:: Uso una variabile contenente il percorso dello script
SET my_path="%~dp0"
cd %my_path%

:: Adesso sono nella cartella dependencies
cd ..

:: Genero l'ambiente virtuale e lo attivo
py -3.8 -m venv .\App\Polibot_venv
call .\App\Polibot_venv\Scripts\activate.bat

:: Adesso che ho attivato l'ambiente virtuale devo installare i moduli di python
python -m pip install --upgrade pip
python -m pip install -U pip setuptools wheel
pip install --no-cache-dir -r .\Dependencies\requirements.txt