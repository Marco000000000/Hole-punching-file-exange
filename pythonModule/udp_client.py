import requests
import json
TURNSERVER="localhost"
# data={
#                 "username":self.user,
#                 "code":dmLy,
#                 "peer_username":peer_username,
#                 "peer_code":peer_code,
#                 "operation":4,
#                 "path":""
#             }
# data={
#                 "username":"marco",
#                 "password":"password",

#             }
# print("http://"+TURNSERVER+":"+"/request/")
# firstCall=requests.post( "http://"+TURNSERVER+"/login", data=json.dumps(data).encode('utf-8'),timeout=5)
# print(firstCall.json())
# code=firstCall.json()["code"]
data={
                "username":"marco2",
                "code":"dmLy",
                "peer_username":"marco",
                "peer_code":"cAFk",
                "operation":3,
                "path":"/"
}
firstCall=requests.post( "http://"+TURNSERVER+"/request", data=json.dumps(data).encode('utf-8'),timeout=5)
print(firstCall.json())
