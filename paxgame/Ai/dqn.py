from __future__ import absolute_import, division, print_function

import base64
import matplotlib
import matplotlib.pyplot as plt
import numpy as np
import gym
import gym_paxgame4
import os
import matplotlib.pyplot as plt
import matplotlib.ticker

import tensorflow as tf

# os.environ['CUDA_VISIBLE_DEVICES'] = '-1'

# Create the environment
env = gym.make("paxgame4-v0")
# low = env.observation_space.low
# high = env.observation_space.high

modelpath = '/data/ai/models/'
modelname = 'dqnboardmechanics'

class DDDQN(tf.keras.Model):
    def __init__(self):
      super(DDDQN, self).__init__()
      self.d1 = tf.keras.layers.Dense(1125, activation='relu')
      self.d2 = tf.keras.layers.Dense(1125, activation='relu')
      self.v = tf.keras.layers.Dense(1, activation=None)
      self.a = tf.keras.layers.Dense(env.action_space.n, activation=None)

    def call(self, input_data):
      x = self.d1(input_data)
      x = self.d2(x)
      v = self.v(x)
      a = self.a(x)
      Q = v +(a -tf.math.reduce_mean(a, axis=1, keepdims=True))
      return Q

    def advantage(self, state):
      x = self.d1(state)
      x = self.d2(x)
      a = self.a(x)
      return a

class exp_replay():
    def __init__(self, buffer_size= 1000000):
        self.buffer_size = buffer_size
        # self.state_mem = np.zeros((self.buffer_size, *(env.observation_space.shape)), dtype=np.float32)
        self.state_mem = np.zeros((self.buffer_size, env.observation_space.n), dtype=np.float32)
        self.action_mem = np.zeros((self.buffer_size), dtype=np.int32)
        self.reward_mem = np.zeros((self.buffer_size), dtype=np.float32)
        # self.next_state_mem = np.zeros((self.buffer_size, *(env.observation_space.shape)), dtype=np.float32)
        self.next_state_mem = np.zeros((self.buffer_size, env.observation_space.n), dtype=np.float32)
        self.done_mem = np.zeros((self.buffer_size), dtype=np.bool)
        self.pointer = 0

    def add_exp(self, state, action, reward, next_state, done):
        idx  = self.pointer % self.buffer_size 
        self.state_mem[idx] = state
        self.action_mem[idx] = action
        self.reward_mem[idx] = reward
        self.next_state_mem[idx] = next_state
        self.done_mem[idx] = 1 - int(done)
        self.pointer += 1

    def sample_exp(self, batch_size):
        max_mem = min(self.pointer, self.buffer_size)
        batch = np.random.choice(max_mem, batch_size, replace=False)
        states = self.state_mem[batch]
        actions = self.action_mem[batch]
        rewards = self.reward_mem[batch]
        next_states = self.next_state_mem[batch]
        dones = self.done_mem[batch]
        return states, actions, rewards, next_states, dones      


class agent():
      def __init__(self, gamma=0.99, replace=100, lr=0.0005):
          self.gamma = gamma
          self.epsilon = 1.0
          # self.min_epsilon = 0.01
          self.min_epsilon = 0.01
          # self.epsilon_decay = 1e-3
          self.epsilon_decay = 1e-4
          self.replace = replace
          self.trainstep = 0
          self.memory = exp_replay()
          # self.batch_size = 64
          self.batch_size = 64
          self.q_net = DDDQN()
          self.target_net = DDDQN()
          opt = tf.keras.optimizers.Adam(learning_rate=lr)
          self.q_net.compile(loss='mse', optimizer=opt)
          self.target_net.compile(loss='mse', optimizer=opt)    

      def act(self, state):
          if np.random.rand() <= self.epsilon:
              return np.random.choice([i for i in range(env.action_space.n)])

          else:
              actions = self.q_net.advantage(np.array([state]))
              action = np.argmax(actions)
              return action              

      def act_filtered(self, state, legal_moves):
          if np.random.rand() <= self.epsilon:
              return np.random.choice([i for i in range(env.action_space.n) if not legal_moves[i]])

          else:
              actions = self.q_net.advantage(np.array([state]))
              masked_actions = np.ma.masked_array(actions, legal_moves)
              # action = np.argmax(actions)
              action = np.argmax(masked_actions)
              return action

      def act_masked(self, state, mask):
          if np.random.rand() <= self.epsilon:
              return np.random.choice([i for i in range(env.action_space.n) if mask[i] == 0])

          else:
              actions = self.q_net.advantage(np.array([state]))
              masked_actions = np.ma.masked_array(actions, mask)
              # action = np.argmax(actions)
              action = np.argmax(masked_actions)
              return action

      def update_mem(self, state, action, reward, next_state, done):
          self.memory.add_exp(state, action, reward, next_state, done)


      def update_target(self):
          self.target_net.set_weights(self.q_net.get_weights())     

      def update_epsilon(self):
          self.epsilon = self.epsilon - self.epsilon_decay if self.epsilon > self.min_epsilon else self.min_epsilon
          return self.epsilon          

      def train(self):
          if self.memory.pointer < self.batch_size:
             return 
          
          if self.trainstep % self.replace == 0:
             self.update_target()
          states, actions, rewards, next_states, dones = self.memory.sample_exp(self.batch_size)
          target = self.q_net.predict(states)
          next_state_val = self.target_net.predict(next_states)
          max_action = np.argmax(self.q_net.predict(next_states), axis=1)
          batch_index = np.arange(self.batch_size, dtype=np.int32)
          q_target = np.copy(target)  #optional  
          q_target[batch_index, actions] = rewards + self.gamma * next_state_val[batch_index, max_action]*dones
          self.q_net.train_on_batch(states, q_target)
          self.update_epsilon()
          self.trainstep += 1          

def moving(data, value=+1, size=100):
    binary_data = [x == value for x in data]
    # this is wasteful but easy to write...
    return [sum(binary_data[i-size:i])/size for i in range(size, len(data) + 1)]

def show(results, size=500, title='Moving average of game outcomes',
         first_label='First Player Wins', second_label='Second Player Wins', draw_label='Draw'):
    x_values = range(size, len(results) + 1)
    first = moving(results, value=+1, size=size)
    second = moving(results, value=-1, size=size)
    draw = moving(results, value=0, size=size)
    first, = plt.plot(x_values, first, color='red', label=first_label)
    second, = plt.plot(x_values, second, color='blue', label=second_label)
    draw, = plt.plot(x_values, draw, color='grey', label=draw_label)
    plt.xlim([0, len(results)])
    plt.ylim([0, 1])
    plt.title(title)
    plt.legend(handles=[first, second, draw], loc='best')
    ax = plt.gca()
    ax.yaxis.set_major_formatter(matplotlib.ticker.PercentFormatter(xmax=1))
    ax.xaxis.set_major_formatter(matplotlib.ticker.StrMethodFormatter('{x:,.0f}'))
    plt.ylabel(f'Rate over trailing window of {size} games')
    plt.xlabel('Game Number')
    plt.show()

agentoo7 = agent()
steps = 80000
gameresults = []
th = ((env.observation_space.n / 2) - 3) * 2 * 0.8
print(th)

for s in range(steps):
  done = False
  state = env.reset()
  total_reward = 0
  actions = []
  rngactions = []
  player = 1
  if s == 40:
    agentoo7.q_net.load_weights(modelpath + modelname + str(env.action_space.n) + '.h5')
  if s % 1000 == 0:
    agentoo7.q_net.save_weights(modelpath + modelname + str(env.action_space.n) + '_' + str(s) + '.h5')

  while not done:
    # print(env.render())
    # action = agentoo7.act_filtered(state, env.legal_moves())
    if player == +1:
        action = agentoo7.act(state)
        # action = agentoo7.act_masked(state, env.players[+1].mask)
        actions.append(action)
        next_state, reward, done, _ = env.step(player, action)
        agentoo7.update_mem(state, action, reward, next_state, done)
        agentoo7.train()
        total_reward += reward
        # if reward == -1:
        #     print("indahouse1")
    elif player == -1:
        dotnetmoves = env.opp_moves()
        for m in range(len(dotnetmoves)):
            # action = np.random.choice([i for i in range(0, env.action_space.n) if i not in rngactions])
            action = dotnetmoves[m]
            # rngactions += env.rng_actions_block(action)
            next_state, reward, done, _ = env.step(player, action)
            # if reward == -1:
            #     print("indahouse2")
    state = next_state
    
    player = player * -1

    if done:
        if total_reward > 10:
            gameresults.append(1)
        elif total_reward < 5:
            gameresults.append(-1)
        else:
            gameresults.append(0)        
        print("total reward after {} episode is {} and epsilon is {} {}".format(s, total_reward, agentoo7.epsilon, actions))
        # print(env.render())

# agentoo7.target_net.save('/data/ai/models/dqntest2')
# agentoo7.target_net.save_weights('/data/ai/models/dqntest2.h5')

agentoo7.q_net.save(modelpath + modelname + str(env.action_space.n))
agentoo7.q_net.save_weights(modelpath + modelname + str(env.action_space.n) + '.h5')


show(gameresults, size=int(steps / 20))      
