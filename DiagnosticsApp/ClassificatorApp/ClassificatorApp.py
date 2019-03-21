import socket
import time
from classification_module import analize
 
serversocket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
serversocket.bind(('localhost', 8080))
serversocket.listen(5)
print("Server is waiting for connections.")
while True:
    # подключение
    clientConnection,clientAddress = serversocket.accept()
    print("Connected clinet :" , clientAddress)
    #TODO сделать как if(data[0]==1) ==2 ==3
    data = clientConnection.recv(1024)
    path_orig = ""
    path_parsed = ""
    
    message = data.decode()
    st = 0

    for i in range(len(data)):
        if (message[i] == '\x00'):
            st = i
            break
        path_orig+=message[i]

    flag = False
    for i in range(st, len(data)):
        if(flag == False):
            if (message[i] != '\x00'):
                flag = True
        if(flag==True):
            if (message[i] == '\x00'):
                break
            path_parsed+=message[i]

    # анализ
    analize(path_orig, path_parsed)
    clientConnection.send(bytes("Successfully analized on Server",'UTF-8'))
    

    clientConnection.close()
    # Делаем задержку, чтобы цикл не сильно загружал процессор
    time.sleep(0.1)