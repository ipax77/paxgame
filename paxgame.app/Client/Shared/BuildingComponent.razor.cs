using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using paxgame.app.Client.Models;
using paxgame.app.Client.Services;
using paxgame.Shared;
using paxgame.Shared.Services;
using System.Globalization;
using System.Net.Http.Json;

namespace paxgame.app.Client.Shared
{
    public partial class BuildingComponent : ComponentBase, IDisposable
    {
        [Inject]
        protected PlayerService playerService { get; set; }

        [Inject]
        protected ILogger<BuildingComponent> logger { get; set; }

        [Inject]
        protected IJSRuntime _js { get; set; }

        [Inject]
        protected HttpClient Http { get; set; }

        private DotNetObjectReference<BuildingComponent>? objRef;
        double buildX = 25;
        double buildY = 15;
        double offsetX = 1000;
        double offsetY = 600;

        bool buildToggle = false;

        private GameResult? gameResult;
        private PlayerUnit? highlightUnit;
        private PlayerUnit? selectedUnit;

        int AnimationDuration = 15;

        bool startPlaying = false;
        bool Playing = false;

        string player1Moves;
        string player2Moves;

        protected override async Task OnInitializedAsync()
        {
            objRef = DotNetObjectReference.Create(this);
            await playerService.GetUnits();
        }

        protected override Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                GetBuildSize();
            }
            return base.OnAfterRenderAsync(firstRender);
        }

        private async Task GetBuildSize()
        {
            var result = await _js.InvokeAsync<BoundingClientRect>("getBuildSize", "builddiv", objRef);
            if (result != null)
            {
                offsetX = result.Width;
                offsetY = result.Height;
                logger.LogInformation($"got build size: {offsetX}|{offsetY}");
            }
        }

        private void DebugClear()
        {
            playerService.Units.Clear();
            playerService.Moves.Clear();
            playerService.OpponentUnits.Clear();
            playerService.OpponentMoves.Clear();
            playerService.Minerals = 0;
            gameResult = null;
            StateHasChanged();
        }

        private void SetPlayerMoves()
        {
            playerService.Moves = player1Moves.Split(",").Select(s => int.Parse(s)).ToHashSet();
            playerService.Units = playerService.Moves.Select(s => GetUnit(s, "Terran")).ToList();
            playerService.OpponentMoves = player2Moves.Split(",").Select(s => int.Parse(s)).ToHashSet();
            playerService.OpponentUnits = playerService.OpponentMoves.Select(s => GetUnit(s, "Zerg")).ToList();
            StateHasChanged();
        }

        private void UnitSelected(UnitResult? unit)
        {
            logger.LogInformation($"unit selected: {unit?.Name}");
            playerService.SelectedUnit = unit;
            highlightUnit = null;
            selectedUnit = null;
            StateHasChanged();
        }

        private void BuildUnit(MouseEventArgs e)
        {
            if (buildToggle)
			{
                return;
			}
            var buildPos = new myVector2() { X = Convert.ToInt32(e.OffsetX / offsetX * buildX), Y = Convert.ToInt32(e.OffsetY / offsetY * buildY) };
            if (playerService.SelectedUnit != null)
            {
                
                if (buildPos.X == buildX || buildPos.Y == buildY)
                {
                    return;
                }
                PlaceUnit(playerService.SelectedUnit, buildPos);                
            } else if (selectedUnit != null)
            {
                RemoveUnit(selectedUnit);
                if (PlaceUnit(selectedUnit.Unit, buildPos))
                {
                    selectedUnit = playerService.Units.Last();
                }
            }
        }

        private void RemoveUnit(PlayerUnit unit)
        {
            if (!unit.Fixed)
            {
                int move = AiService.GetUnitMove(unit.Unit.UnitId, unit.BuildPos);
                playerService.Moves.Remove(move);
                playerService.Units.Remove(unit);
            }
        }

        private void Undo()
        {
            var unit = playerService.Units.Last();
            if (!unit.Fixed)
            {
                RemoveUnit(unit);
            }
        }

        private bool PlaceUnit(UnitResult unit, myVector2 buildPos)
        {
            var occupied = playerService.Units.FirstOrDefault(f => f.BuildPos.Equals(buildPos));
            if (occupied != null)
            {
                return false;
            } else
            {
                var screenPos = new myVector2() { X = Convert.ToInt32(buildPos.X * 100f / buildX), Y = Convert.ToInt32(buildPos.Y * 100f / buildY) };
                playerService.Units.Add(new PlayerUnit(unit, buildPos, screenPos));
                playerService.Minerals -= unit.Cost;
                playerService.Moves.Add(AiService.GetUnitMove(unit.UnitId, buildPos));
                logger.LogInformation($"move: {unit.UnitId}|{buildPos.X}|{buildPos.Y} => {playerService.Moves.Last()}");
                (int unitId, myVector2 bp) = AiService.GetUnitPos(playerService.Moves.Last());
                logger.LogInformation($"unit: {unitId} => {bp.X}|{bp.Y}");
                return true;
            }
        }

        private async Task Animate()
        {
            Play();
            buildToggle = false;
            startPlaying = true;
            await InvokeAsync(() => StateHasChanged());
        }

        private async Task Play()
        {
            var request = new GameResultRequest(Guid.NewGuid(), playerService.Moves, playerService.OpponentMoves, offsetX, offsetY, AnimationDuration);
            var response = await Http.PostAsJsonAsync("api/v1/paxgame/play", request);
            if (response.IsSuccessStatusCode)
            {
                gameResult = await response.Content.ReadFromJsonAsync<GameResult>();
                if (gameResult != null)
                {
                    playerService.Units.ForEach(f => f.Fixed = true);
                    playerService.OpponentMoves = gameResult.PlayerMoves[1].ToHashSet();
                    playerService.OpponentUnits = gameResult.PlayerMoves[1].Select(s => GetUnit(s, "Zerg")).ToList();
                    startPlaying = false;
                    Playing = true;
                    await InvokeAsync(() => StateHasChanged());
                }
            }
        }

        private void StopPlaying()
        {
            Playing = false;
            gameResult = null;
            InvokeAsync(() => StateHasChanged());
        }

        private PlayerUnit GetUnit(int move, string race)
        {
            (int unitId, myVector2 buildPos) = AiService.GetUnitPos(move);
            var screenPos = new myVector2() { X = Convert.ToInt32(buildPos.X * 100f / buildX), Y = Convert.ToInt32(buildPos.Y * 100f / buildY) };
            if (race == "Zerg")
            {
                return new PlayerUnit(playerService.AvailabelOpponentUnits.First(f => f.UnitId == unitId), buildPos, screenPos);
            } else if (race == "Terran")
            {
                return new PlayerUnit(playerService.AvailabelUnits.First(f => f.UnitId == unitId), buildPos, screenPos);
            } else
            {
                return new PlayerUnit(playerService.AvailabelUnits.First(f => f.UnitId == unitId), buildPos, screenPos);
            }
            
        }

        private void MouseOver(PlayerUnit unit)
        {
            if (playerService.SelectedUnit == null)
            {
                highlightUnit = unit;
            }
        }

        private void UnitClick(PlayerUnit unit)
        {
            logger.LogInformation($"unit clicked: {unit.Unit.Name} ({unit.ScreenPos.X}|{unit.ScreenPos.Y})");
            if (playerService.SelectedUnit != null)
            {
                myVector2 buildPos = new myVector2(unit.BuildPos.Pos);
                var x = buildPos.X;
                var y = buildPos.Y;
                bool posFound = false;
                for (int i = 1; i < buildX * buildY; i++)
                {
                    var mx = x;
                    var my = y;

                    for (int j = 0; j < 4; j++)
                    {
                        if (j == 0)
                        {
                            mx = x + i;
                            if (mx >= buildX)
                            {
                                x = 0;
                            }
                        }
                        if (j == 1)
                        {
                            mx = x - i;
                            if (mx < 0)
                            {
                                mx = (float)buildX - 1;
                            }
                        }
                        if (j == 2)
                        {
                            my = y + i;
                            if (my >= buildY)
                            {
                                my = 0;
                            }
                        }
                        if (j == 3)
                        {
                            my = y - i;
                            if (my < 0)
                            {
                                my = (float)buildY - 1;
                            }
                        }
                        buildPos = new myVector2() { X = mx, Y = my };
                        posFound = PlaceUnit(playerService.SelectedUnit, buildPos);
                        if (posFound)
                        {
                            break;
                        } else
                        {
                            mx = x;
                            my = y;
                        }
                    }
                    if (posFound)
                    {
                        break;
                    }
                }
            }
            else
            {
                selectedUnit = unit;
            }
        }
    
        [JSInvokable]
        public void GetResizeInfo(BoundingClientRect rect)
        {
            offsetX = rect.Width;
            offsetY = rect.Height;
            logger.LogInformation($"setting size to {offsetX}|{offsetY}");
        }

        public void Dispose()
        {
            objRef?.Dispose();
            _js.InvokeVoidAsync("dotnetDispose");
        }
    }
}
