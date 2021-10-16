from gym.envs.registration import register
 
register(id='paxgame4-v0', 
    entry_point='gym_paxgame4.envs:paxgame4Env', 
)