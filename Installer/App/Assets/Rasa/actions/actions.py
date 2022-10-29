from typing import Any, Text, Dict, List

from rasa_sdk import Action, Tracker
from rasa_sdk.executor import CollectingDispatcher

import random
import string

responses_class_A = ("L'aula A si trova al secondo piano dell'edificio che si affaccia sull'atrio Cherubini, lato ovest.",
                    "L'aula A è al secondo piano dell'edificio che si affaccia sull'atrio Cherubini, lato ovest.")

responses_class_B = ("L'aula B si trova al secondo piano dell'edificio che si affaccia sull'atrio Cherubini, lato ovest.",
                    "L'aula B è al secondo piano dell'edificio che si affaccia sull'atrio Cherubini, lato ovest.")

responses_class_C = ("L'aula C si trova al secondo piano dell'edificio che si affaccia sull'atrio Cherubini, lato ovest.",
                    "L'aula C è al secondo piano dell'edificio che si affaccia sull'atrio Cherubini, lato ovest.")

responses_class_D = ("L'aula D si trova al secondo piano dell'edificio che si affaccia sull'atrio Cherubini, lato est.",
                    "L'aula D è al secondo piano dell'edificio che si affaccia sull'atrio Cherubini, lato est.")

responses_class_E = ("L'aula E si trova al secondo piano dell'edificio che si affaccia sull'atrio Cherubini, lato est.",
                    "L'aula E è al secondo piano dell'edificio che si affaccia sull'atrio Cherubini, lato est.")

responses_class_F = ("L'aula F si trova al secondo piano dell'edificio che si affaccia sull'atrio Cherubini, lato est.",
                    "L'aula F è al secondo piano dell'edificio che si affaccia sull'atrio Cherubini, lato est.")

responses_class_G = ("L'aula G si trova al secondo piano dell'edificio che si affaccia sull'atrio Cherubini, lato ovest.",
                    "L'aula G è al secondo piano dell'edificio che si affaccia sull'atrio Cherubini, lato ovest.")

responses_class_H = ("L'aula H si trova al secondo piano dell'edificio che si affaccia sull'atrio Cherubini, lato ovest.",
                    "L'aula H è al secondo piano dell'edificio che si affaccia sull'atrio Cherubini, lato ovest.")

responses_class_I = ("L'aula I si trova al secondo piano dell'edificio che si affaccia sull'atrio Cherubini, lato ovest.",
                    "L'aula I è al secondo piano dell'edificio che si affaccia sull'atrio Cherubini, lato ovest.")

responses_class_L = ("L'aula L si trova al secondo piano dell'edificio che si affaccia sull'atrio Cherubini, lato est.",
                    "L'aula L è al secondo piano dell'edificio che si affaccia sull'atrio Cherubini, lato est.")

responses_class_M = ("L'aula M si trova al secondo piano dell'edificio che si affaccia sull'atrio Cherubini, lato est.",
                    "L'aula M è al secondo piano dell'edificio che si affaccia sull'atrio Cherubini, lato est.")

responses_class_N = ("L'aula N si trova al secondo piano dell'edificio che si affaccia sull'atrio Cherubini, lato est.",
                    "L'aula N è al secondo piano dell'edificio che si affaccia sull'atrio Cherubini, lato est.")

responses_class_O = ("L'aula O si trova al piano terra dell'edificio che si affaccia sull'atrio Cherubini, lato nord.",
                    "L'aula O è al piano terra dell'edificio che si affaccia sull'atrio Cherubini, lato nord.")

responses_class_P = ("L'aula P si trova al piano terra dell'edificio che si affaccia sull'atrio Cherubini, lato nord.",
                    "L'aula P è al piano terra dell'edificio che si affaccia sull'atrio Cherubini, lato nord.")

responses_class_Q = ("L'aula Q si trova al piano terra dell'edificio che si affaccia sull'atrio Cherubini, lato nord.",
                    "L'aula Q è al piano terra dell'edificio che si affaccia sull'atrio Cherubini, lato nord.")

responses_class_R = ("L'aula R si trova al piano terra dell'edificio che si affaccia sull'atrio Cherubini, lato nord.",
                    "L'aula R è al piano terra dell'edificio che si affaccia sull'atrio Cherubini, lato nord.")

responses_class_S = ("L'aula S si trova al piano terra dell'edificio che si affaccia sull'atrio Cherubini, lato nord.",
                    "L'aula S è al piano terra dell'edificio che si affaccia sull'atrio Cherubini, lato nord.")

responses_language_class = ("Il centro linguistico è al terzo piano dell'edificio in fondo al viale.",
                            "Il centro linguistico è nell'edificio in fondo al viale, al terzo piano.")

responses_class_AD = ("L'aula AD si affaccia al viale al piano terra, lato ovest.",
                     "L'aula AD si raggiunge tramite il viale al piano terra, lato ovest.")

#Le aule numerate da 1 a 20 hanno tutte la stessa risposta, in futuro queste potranno essere cambiate per essere più specifiche
responses_class_1 = ("Le aule numerate da 1 a 20 sono al secondo piano dell'edificio sopra il corridoio con le sculture. Una volta raggiunto il secondo piano dell'edificio basta seguire le indicazioni.",
                    "Le aule numerate da 1 a 20 si possono raggiungere tramite le scale situate accanto al gabbiotto del Polìba control. Al secondo piano ci sono le indicazioni per trovare l'aula desiderata.")

responses_class_2 = ("Le aule numerate da 1 a 20 sono al secondo piano dell'edificio sopra il corridoio con le sculture. Una volta raggiunto il secondo piano dell'edificio basta seguire le indicazioni.",
                    "Le aule numerate da 1 a 20 si possono raggiungere tramite le scale situate accanto al gabbiotto del Polìba control. Al secondo piano ci sono le indicazioni per trovare l'aula desiderata.")

responses_class_3 = ("Le aule numerate da 1 a 20 sono al secondo piano dell'edificio sopra il corridoio con le sculture. Una volta raggiunto il secondo piano dell'edificio basta seguire le indicazioni.",
                    "Le aule numerate da 1 a 20 si possono raggiungere tramite le scale situate accanto al gabbiotto del Polìba control. Al secondo piano ci sono le indicazioni per trovare l'aula desiderata.")

responses_class_4 = ("Le aule numerate da 1 a 20 sono al secondo piano dell'edificio sopra il corridoio con le sculture. Una volta raggiunto il secondo piano dell'edificio basta seguire le indicazioni.",
                    "Le aule numerate da 1 a 20 si possono raggiungere tramite le scale situate accanto al gabbiotto del Polìba control. Al secondo piano ci sono le indicazioni per trovare l'aula desiderata.")

responses_class_5 = ("Le aule numerate da 1 a 20 sono al secondo piano dell'edificio sopra il corridoio con le sculture. Una volta raggiunto il secondo piano dell'edificio basta seguire le indicazioni.",
                    "Le aule numerate da 1 a 20 si possono raggiungere tramite le scale situate accanto al gabbiotto del Polìba control. Al secondo piano ci sono le indicazioni per trovare l'aula desiderata.")

responses_class_6 = ("Le aule numerate da 1 a 20 sono al secondo piano dell'edificio sopra il corridoio con le sculture. Una volta raggiunto il secondo piano dell'edificio basta seguire le indicazioni.",
                    "Le aule numerate da 1 a 20 si possono raggiungere tramite le scale situate accanto al gabbiotto del Polìba control. Al secondo piano ci sono le indicazioni per trovare l'aula desiderata.")

responses_class_7 = ("Le aule numerate da 1 a 20 sono al secondo piano dell'edificio sopra il corridoio con le sculture. Una volta raggiunto il secondo piano dell'edificio basta seguire le indicazioni.",
                    "Le aule numerate da 1 a 20 si possono raggiungere tramite le scale situate accanto al gabbiotto del Polìba control. Al secondo piano ci sono le indicazioni per trovare l'aula desiderata.")

responses_class_8 = ("Le aule numerate da 1 a 20 sono al secondo piano dell'edificio sopra il corridoio con le sculture. Una volta raggiunto il secondo piano dell'edificio basta seguire le indicazioni.",
                    "Le aule numerate da 1 a 20 si possono raggiungere tramite le scale situate accanto al gabbiotto del Polìba control. Al secondo piano ci sono le indicazioni per trovare l'aula desiderata.")

responses_class_9 = ("Le aule numerate da 1 a 20 sono al secondo piano dell'edificio sopra il corridoio con le sculture. Una volta raggiunto il secondo piano dell'edificio basta seguire le indicazioni.",
                    "Le aule numerate da 1 a 20 si possono raggiungere tramite le scale situate accanto al gabbiotto del Polìba control. Al secondo piano ci sono le indicazioni per trovare l'aula desiderata.")

responses_class_10 = ("Le aule numerate da 1 a 20 sono al secondo piano dell'edificio sopra il corridoio con le sculture. Una volta raggiunto il secondo piano dell'edificio basta seguire le indicazioni.",
                    "Le aule numerate da 1 a 20 si possono raggiungere tramite le scale situate accanto al gabbiotto del Polìba control. Al secondo piano ci sono le indicazioni per trovare l'aula desiderata.")

responses_class_11 = ("Le aule numerate da 1 a 20 sono al secondo piano dell'edificio sopra il corridoio con le sculture. Una volta raggiunto il secondo piano dell'edificio basta seguire le indicazioni.",
                    "Le aule numerate da 1 a 20 si possono raggiungere tramite le scale situate accanto al gabbiotto del Polìba control. Al secondo piano ci sono le indicazioni per trovare l'aula desiderata.")

responses_class_12 = ("Le aule numerate da 1 a 20 sono al secondo piano dell'edificio sopra il corridoio con le sculture. Una volta raggiunto il secondo piano dell'edificio basta seguire le indicazioni.",
                    "Le aule numerate da 1 a 20 si possono raggiungere tramite le scale situate accanto al gabbiotto del Polìba control. Al secondo piano ci sono le indicazioni per trovare l'aula desiderata.")

responses_class_13 = ("Le aule numerate da 1 a 20 sono al secondo piano dell'edificio sopra il corridoio con le sculture. Una volta raggiunto il secondo piano dell'edificio basta seguire le indicazioni.",
                    "Le aule numerate da 1 a 20 si possono raggiungere tramite le scale situate accanto al gabbiotto del Polìba control. Al secondo piano ci sono le indicazioni per trovare l'aula desiderata.")

responses_class_14 = ("Le aule numerate da 1 a 20 sono al secondo piano dell'edificio sopra il corridoio con le sculture. Una volta raggiunto il secondo piano dell'edificio basta seguire le indicazioni.",
                    "Le aule numerate da 1 a 20 si possono raggiungere tramite le scale situate accanto al gabbiotto del Polìba control. Al secondo piano ci sono le indicazioni per trovare l'aula desiderata.")

responses_class_15 = ("Le aule numerate da 1 a 20 sono al secondo piano dell'edificio sopra il corridoio con le sculture. Una volta raggiunto il secondo piano dell'edificio basta seguire le indicazioni.",
                    "Le aule numerate da 1 a 20 si possono raggiungere tramite le scale situate accanto al gabbiotto del Polìba control. Al secondo piano ci sono le indicazioni per trovare l'aula desiderata.")

responses_class_16 = ("Le aule numerate da 1 a 20 sono al secondo piano dell'edificio sopra il corridoio con le sculture. Una volta raggiunto il secondo piano dell'edificio basta seguire le indicazioni.",
                    "Le aule numerate da 1 a 20 si possono raggiungere tramite le scale situate accanto al gabbiotto del Polìba control. Al secondo piano ci sono le indicazioni per trovare l'aula desiderata.")

responses_class_17 = ("Le aule numerate da 1 a 20 sono al secondo piano dell'edificio sopra il corridoio con le sculture. Una volta raggiunto il secondo piano dell'edificio basta seguire le indicazioni.",
                    "Le aule numerate da 1 a 20 si possono raggiungere tramite le scale situate accanto al gabbiotto del Polìba control. Al secondo piano ci sono le indicazioni per trovare l'aula desiderata.")

responses_class_18 = ("Le aule numerate da 1 a 20 sono al secondo piano dell'edificio sopra il corridoio con le sculture. Una volta raggiunto il secondo piano dell'edificio basta seguire le indicazioni.",
                    "Le aule numerate da 1 a 20 si possono raggiungere tramite le scale situate accanto al gabbiotto del Polìba control. Al secondo piano ci sono le indicazioni per trovare l'aula desiderata.")

responses_class_19 = ("Le aule numerate da 1 a 20 sono al secondo piano dell'edificio sopra il corridoio con le sculture. Una volta raggiunto il secondo piano dell'edificio basta seguire le indicazioni.",
                    "Le aule numerate da 1 a 20 si possono raggiungere tramite le scale situate accanto al gabbiotto del Polìba control. Al secondo piano ci sono le indicazioni per trovare l'aula desiderata.")

responses_class_20 = ("Le aule numerate da 1 a 20 sono al secondo piano dell'edificio sopra il corridoio con le sculture. Una volta raggiunto il secondo piano dell'edificio basta seguire le indicazioni.",
                    "Le aule numerate da 1 a 20 si possono raggiungere tramite le scale situate accanto al gabbiotto del Polìba control. Al secondo piano ci sono le indicazioni per trovare l'aula desiderata.")

responses_class_21 = ("L'aula 21 si trova al primo piano dell'edificio che si affaccia sull'atrio Cherubini, lato ovest.",
                    "L'aula 21 è al primo piano dell'edificio che si affaccia sull'atrio Cherubini, lato ovest.")

responses_class_22 = ("L'aula 22 si trova al primo piano dell'edificio che si affaccia sull'atrio Cherubini, lato ovest.",
                    "L'aula 22 è al primo piano dell'edificio che si affaccia sull'atrio Cherubini, lato ovest.")

responses_class_23 = ("L'aula 23 si trova al primo piano dell'edificio che si affaccia sull'atrio Cherubini, lato ovest.",
                    "L'aula 23 è al primo piano dell'edificio che si affaccia sull'atrio Cherubini, lato ovest.")

responses_class_24 = ("L'aula 24 si trova al primo piano dell'edificio che si affaccia sull'atrio Cherubini, lato est.",
                    "L'aula 24 è al primo piano dell'edificio che si affaccia sull'atrio Cherubini, lato est.")

responses_class_25 = ("L'aula 25 si trova al primo piano dell'edificio che si affaccia sull'atrio Cherubini, lato est.",
                    "L'aula 25 è al primo piano dell'edificio che si affaccia sull'atrio Cherubini, lato est.")

responses_class_26 = ("L'aula 26 si trova al primo piano dell'edificio che si affaccia sull'atrio Cherubini, lato est.",
                    "L'aula 26 è al primo piano dell'edificio che si affaccia sull'atrio Cherubini, lato est.")

responses = {"A":responses_class_A,
             "B":responses_class_B,
             "C":responses_class_C,
             "D":responses_class_D,
             "E":responses_class_E,
             "F":responses_class_F,
             "G":responses_class_G,
             "H":responses_class_H,
             "I":responses_class_I,
             "L":responses_class_L,
             "M":responses_class_M,
             "N":responses_class_N,
             "O":responses_class_O,
             "P":responses_class_P,
             "Q":responses_class_Q,
             "R":responses_class_R,
             "S":responses_class_S,
             "centro linguistico":responses_language_class,
             "AD":responses_class_AD,
             "1":responses_class_1,
             "2":responses_class_2,
             "3":responses_class_3,
             "4":responses_class_4,
             "5":responses_class_5,
             "6":responses_class_6,
             "7":responses_class_7,
             "8":responses_class_8,
             "9":responses_class_9,
             "10":responses_class_10,
             "11":responses_class_11,
             "12":responses_class_12,
             "13":responses_class_13,
             "14":responses_class_14,
             "15":responses_class_15,
             "16":responses_class_16,
             "17":responses_class_17,
             "18":responses_class_18,
             "19":responses_class_19,
             "20":responses_class_20,
             "21":responses_class_21,
             "22":responses_class_22,
             "23":responses_class_23,
             "24":responses_class_24,
             "25":responses_class_25,
             "26":responses_class_26}

class ActionClassAnswer(Action):

    def name(self) -> Text:
        return "action_class_answer"

    def run(self, dispatcher: CollectingDispatcher,
            tracker: Tracker,
            domain: Dict[Text, Any]) -> List[Dict[Text, Any]]:

        class_id = next(tracker.get_latest_entity_values('aula'), None)

        try:
            responses_max_index = len(responses[class_id]) - 1
            dispatcher.utter_message(text=responses[class_id][random.randint(0, responses_max_index)])
        except KeyError:
            dispatcher.utter_message("La classe richiesta non esiste.")

        return []