import requests
import uuid
import json

api_url = "http://localhost:5161/api/v1/paxgame"
headers =  {"Content-Type":"application/json"}
api_request = {"guid": str(uuid.uuid4()), "moves": [[1,2,3,4,5],[]] }
response = requests.post(api_url, data=json.dumps(api_request), headers=headers)
print (response.status_code)
result = response.json()
print (result['reward'])

