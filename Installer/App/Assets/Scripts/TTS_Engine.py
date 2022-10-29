from win32event import CreateMutex
from win32api import GetLastError
from winerror import ERROR_ALREADY_EXISTS

# I check whether or not there is anotehr instance of the same script running.
# If this is the first instance I start the server otehrwise i close the script.
# In order to check this I have to use a Mutex.
handle = CreateMutex(None, 1, 'TTS_Engine mutex')

if GetLastError() == ERROR_ALREADY_EXISTS:
    # An instance of this script is already running
    exit()

#This is the first instance of this script
import clr
clr.AddReference('System.Speech')
from System.Speech.Synthesis import SpeechSynthesizer, InstalledVoice, VoiceInfo, SynthesizerState

from http.server import BaseHTTPRequestHandler, HTTPServer
import requests
import json
    
synth = SpeechSynthesizer()
synth.SetOutputToDefaultAudioDevice()

try:
    synth.SelectVoice('Microsoft Cosimo')
except Exception as e:
    print(e)

class voice_RequestHandler(BaseHTTPRequestHandler):
    def do_POST(self):
        # Response to the sender
        self.send_response(200)
        self.send_header('Content-type','text/html')
        self.end_headers()
        message = str("OK")
        self.wfile.write(bytes(message, "utf8"))

        # Read the message and make the voice speak
        content_length = int(self.headers['Content-Length'])
        post_data = self.rfile.read(content_length)
        # post_data is a string of bytes and has to be decoded
        post_data = post_data.decode('utf-8')
        post_dict = json.loads(post_data)
        message = post_dict['message']
        synth.SpeakAsync(message)
        return
    
    def do_GET(self):
        # Specifichiamo il codice di risposta
        self.send_response(200)
        # Specifichiamo uno o più header
        self.send_header('Content-type','text/html')
        self.end_headers()
        # Specifichiamo il messaggio che costituirà il corpo della risposta
        message = str(synth.State == SynthesizerState.Speaking)
        self.wfile.write(bytes(message, "utf8"))
        return
    
    def run():
        print('Starting voice server...')
        server_address = ('127.0.0.1', 8081)
        httpd = HTTPServer(server_address, voice_RequestHandler)
        print('Running voice server...')
        httpd.serve_forever()

voice_RequestHandler.run()