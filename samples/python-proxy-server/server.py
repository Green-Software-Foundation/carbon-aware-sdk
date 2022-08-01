import sys,_thread as thread,socket, logging, re

logging.basicConfig(format='%(asctime)s - %(levelname)s: %(message)s', level=logging.INFO)

class Server: 
    BACKLOG = 50            # how many pending connections queue will hold
    MAX_DATA_RECV = 4096    # max number of bytes we receive at once
    DEBUG = False           # set to True to see the debug msgs

    def __init__(self, port):
        # Create a TCP socket
        self.server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

        # Re-use the socket
        self.server.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)

        # bind the socket to localhost, and a port   
        self.server.bind(('', port))

        # become a server socket        
        self.server.listen(self.BACKLOG) 
        logging.info(f'Proxy server listening on port {port}')

    def serve(self):
        # get the connection from client
        while True:
            conn, client_addr = self.server.accept()
            logging.info(f'Connected to client at {client_addr}')

            # create a thread to handle request
            thread.start_new_thread(self.proxy_thread, (conn, client_addr))

        self.server.close()

    def proxy_thread(self, conn, client_addr):
        # get the request from client
        request = conn.recv(self.MAX_DATA_RECV)
        stringRequest = request.decode('utf-8')

        # parse the first line
        first_line = stringRequest.split('\n')[0]

        # get url
        url = first_line.split(' ')[1]

        if (self.DEBUG):
            print(first_line)
            print("URL:", url)

        # find the webserver and port
        http_pos = url.find("://")          # find pos of ://
        if (http_pos==-1):
            temp = url
        else:
            temp = url[(http_pos+3):]       # get the rest of url

        port_pos = temp.find(":")           # find the port pos (if any)

        # find end of web server
        webserver_pos = temp.find("/")
        if webserver_pos == -1:
            webserver_pos = len(temp)

        webserver = ""
        port = -1
        if (port_pos==-1 or webserver_pos < port_pos):      # default port
            port = 80
            webserver = temp[:webserver_pos]
        else:       # specific port
            port = int((temp[(port_pos+1):])[:webserver_pos-port_pos-1])
            webserver = temp[:port_pos]
        
        logging.info(f'Client requesting connection to {webserver}')

        try:
            # create a socket to connect to the web server
            s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            logging.info(f'Sending client request to {webserver}')
            s.connect((webserver, port))
            s.send(request)         # send request to webserver
            first_read = True
            while True:
                # receive data from web server
                data = s.recv(self.MAX_DATA_RECV)
                if (len(data) > 0):
                    if (first_read):
                        status_code = self.parse_status_code(data)
                        if (status_code != None):
                            logging.info(f'Received HTTP {status_code} response.')
                        first_read = False
                    logging.info('Reading response and sending to client')
                    # send to client
                    logging.debug(f'data<{data}>')
                    conn.send(data)
                else:
                    logging.info('No data received from server')
                    break
            s.close()
            conn.close()
        except socket.error:
            if s:
                s.close()
            if conn:
                conn.close()
            logging.error('Runtime error')
            sys.exit(1)
    def parse_status_code(self, data):
        # parse the first line with status code
        first_line = str(data).split('\n')[0]
        # pull the http header
        http_header  = re.search("(HTTP\/1.1 )[\d]{3}", first_line)
        if (http_header):
            # parse the status code
            return http_header.group().split(" ")[1]
        return None

        

if __name__ == '__main__':
    port = 8080
    server = Server(port)
    try:
        server.serve()
    except KeyboardInterrupt:
        logging.info("Ctrl C - Stopping server")
        sys.exit(1)