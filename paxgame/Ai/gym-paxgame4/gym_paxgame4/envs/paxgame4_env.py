import gym
import numpy as np
import requests
import uuid
from gym import error, spaces, utils, logger
from gym.utils import seeding
import requests
import uuid
import json

x = 25
y = 15
terranunits = 3
zergunits = 4
terranmoves = x * y * terranunits
zergmoves = x * y * zergunits
xy = x * y

api_url = "http://localhost:5161/api/v1/paxgame"
headers =  {"Content-Type":"application/json"}

exit

class paxgame4Env(gym.Env):  
    metadata = {'render.modes': ['human']}   
    def __init__(self):
        self.action_space = spaces.Discrete(terranmoves)
        self.observation_space = spaces.Discrete(xy*2)
        self.round = None
        self.state = None
        self.player = +1
        self.players = { +1: Player("Terran"), -1: Player("Zerg") }
        self.uuid = str(uuid.uuid4())
    
    def unit_move(self, action, race):
        unit, move = divmod(action, xy)
        mod = 1
        if race == "Zerg":
            mod = 100
        elif race == "Protoss":
            mod = 200
        return int((unit+1)*mod), int(move)

    def rng_actions_block(self, action):
        unit, move = divmod(action, xy)
        blocked = []
        for i in range(zergunits):
            blocked.append(int(i * xy + move))
        return blocked

    def step(self, player, action):
        # err_msg = "%r (%s) invalid" % (action, type(action))
        # assert self.action_space.contains(action), err_msg

        # myaction = np.random.choice([i for i in range(moves, moves*2) if not self.state[i]])
        # myunit, mymove = self.unit_move(myaction-moves)
        # self.state[mymove + moves] = myunit
        
        # print (str(self.round) + "|" + str(xy * 2 - 3))
        done = self.round > 15

        splayer = self.players[player]
        unit, move = self.unit_move(action, splayer.race)
        units = terranunits
        if splayer.race == "Zerg":
            units = zergunits

        if splayer.state[move] > 0:
            reward = -1.0
            done = True
        else:
            splayer.state[move] = unit
            # self.state = np.concatenate((splayer.state, self.players[player * -1].state), axis=0)
            self.state = np.concatenate((self.players[+1].state, self.players[-1].state), axis=0)
            splayer.actions.append(int(action))
            reward = 1 # train mechanics only
            # reward = self.step_reward(unit, move)
            for i in range(units):
                splayer.mask[i * xy + move] = 1

        if player == 1:
            # reward *= -1
            self.round = self.round + 1

        # done = self.round >= moves or np.all(self.state)
        

        return np.array(self.state, dtype=np.float32), reward, done, {}
    
    def step_reward(self, unit, move):
        reward = requests.post(api_url, data=json.dumps({"guid": self.uuid, "moves":[self.players[1].actions, self.players[-1].actions]}), headers=headers).json()
        # reward = 0
        # if self.state[move + xy] == unit:
        #     reward = 2.0
        # elif self.state[move + xy] > 0:
        #     reward = 1.5
        # else:
        #     reward = 1.0
        return reward

    def opp_moves(self):
        moves = requests.post(api_url + "/moves", data=json.dumps({"guid": self.uuid, "moves":[self.players[1].actions, self.players[-1].actions]}), headers=headers).json()
        return moves

    def legal_moves(self):
        # return np.where(np.split(self.state, 2)[0] == 0)[0]
        return np.split(self.state, 2)[0]
 
    def reset(self):
        self.uuid = str(uuid.uuid4())
        self.state = np.zeros((xy*2))
        self.round = 0
        self.player = 1
        self.players[+1].reset()
        self.players[-1].reset()
        return np.array(self.state, dtype=np.float32)
 
    def render(self, mode='human', close=False):
        board = "\n"
        for i in range(y):
            rowa = ""
            rowb = ""
            for j in range(x):
                rowa += " " + str(int(self.players[+1].state[i * x + j])) + " |"
                rowb += " " + str(int(self.players[-1].state[i * x + j])) + " |"
            board += "|" + rowa + " X |" + rowb + "\n"
        return board

class Player():
    def __init__ (self, race):
        moves = terranmoves
        if (race == "Zerg"):
            moves = zergmoves
        self.race = race
        self.minerals = 0
        self.actions = []
        self.mask = np.zeros((moves))
        self.state = np.zeros((xy), dtype=np.float32)

    def reset(self):
        moves = terranmoves
        if (self.race == "Zerg"):
            moves = zergmoves
        self.minerals = 0
        self.actions = []
        self.mask = np.zeros((moves))
        self.state = np.zeros((xy), dtype=np.float32)

