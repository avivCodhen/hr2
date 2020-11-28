import textract
import os
from os import listdir

files = os.listdir("hr")
print(files)
for file in files:
    text = textract.process("hr/"+file, method='tesseract',
    language='heb',encoding="ascii")
    print(textract.encode())
