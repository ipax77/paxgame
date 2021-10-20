from gym.envs.registration import register
 
register(id='paxgame5-v0', 
    entry_point='gym_paxgame5.envs:paxgame5Env', 
)