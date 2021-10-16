import gym
import numpy as np
from gym import error, spaces, utils, logger
from gym.utils import seeding

x = 3
y = 6
units = 3
moves = x * y * units
xy = x * y

class paxgame3Env(gym.Env):  
    metadata = {'render.modes': ['human']}   
    def __init__(self):
        self.action_space = spaces.Discrete(moves)
        self.observation_space = spaces.Discrete(xy*2)
        self.round = None
        self.state = None
        self.player = +1
        self.players = { +1: Player(), -1: Player() }
    
    def unit_move(self, action):
        unit, move = divmod(action, xy)
        return int(unit+1), int(move)

    def rng_actions_block(self, action):
        unit, move = divmod(action, xy)
        blocked = []
        for i in range(units):
            blocked.append(int(i * xy + move))
        return blocked

    def step(self, player, action):
        err_msg = "%r (%s) invalid" % (action, type(action))
        assert self.action_space.contains(action), err_msg

        # myaction = np.random.choice([i for i in range(moves, moves*2) if not self.state[i]])
        # myunit, mymove = self.unit_move(myaction-moves)
        # self.state[mymove + moves] = myunit
        
        # print (str(self.round) + "|" + str(xy * 2 - 3))
        done = self.round > xy - 3

        unit, move = self.unit_move(action)

        if self.players[player].state[move] > 0:
            reward = -1.0
            done = True
        else:
            self.players[player].state[move] = unit
            # self.state = np.concatenate((self.players[player].state, self.players[player * -1].state), axis=0)
            self.state = np.concatenate((self.players[+1].state, self.players[-1].state), axis=0)
            reward = self.step_reward(unit, move)
            for i in range(units):
                self.players[player].mask[i * xy + move] = 1

        if player == -1:
            # reward *= -1
            self.round = self.round + 1

        # done = self.round >= moves or np.all(self.state)
        

        return np.array(self.state, dtype=np.float32), reward, done, {}
    
    def step_reward(self, unit, move):
        reward = 0
        if self.state[move + xy] == unit:
            reward = 2.0
        elif self.state[move + xy] > 0:
            reward = 1.5
        else:
            reward = 1.0
        return reward

    def legal_moves(self):
        # return np.where(np.split(self.state, 2)[0] == 0)[0]
        return np.split(self.state, 2)[0]
 
    def reset(self):
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
                rowa += " " + str(self.players[+1].state[i * x + j]) + " |"
                rowb += " " + str(self.players[-1].state[i * x + j]) + " |"
            board += "|" + rowa + " X |" + rowb + "\n"
        return board

class Player():
    def __init__ (self):
        self.minerals = 0
        self.actions = []
        self.mask = np.zeros((moves))
        self.state = np.zeros((xy), dtype=np.float32)

    def reset(self):
        self.minerals = 0
        self.actions = []
        self.mask = np.zeros((moves))
        self.state = np.zeros((xy), dtype=np.float32)

