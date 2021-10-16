from gym.envs.registration import register
 
register(id='paxgame3-v0', 
    entry_point='gym_paxgame3.envs:paxgame3Env', 
)