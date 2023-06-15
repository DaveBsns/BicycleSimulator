import socket
import json
from fastapi import FastAPI

UDP_IP = "10.3.141.1"
UDP_PORT = 8888

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
sock.bind((UDP_IP, UDP_PORT))

app = FastAPI()



@app.get("/data")
def read_root():
    try:
        data, addr = sock.recvfrom(1024)
        print("Recived Data from: " + str(addr))
        parsedJSON = json.loads(data)
        return parsedJSON 
    except sock.timeout:
        sock.close()
        sock.bind((UDP_IP, UDP_PORT))
