import os
import subprocess
from vad2 import VoiceActivityDetector
import numpy as np
from scipy.io.wavfile import read

AUDIO_DIR = 'audio/jokes'
NONAUDIO_DIR = 'audio/nonjokes'


for fname in os.listdir(AUDIO_DIR):
    if fname.endswith('.mp3') or fname.endswith('.wav'):
        fpath = os.path.join(AUDIO_DIR, fname)
        v = VoiceActivityDetector(fpath)
        detected = v.detect_speech()
        out_fpath = os.path.join(AUDIO_DIR, 'processed', fname[:-4] + '.txt')
        np.savetxt(out_fpath, detected)

for fname in os.listdir(NONAUDIO_DIR):
    if fname.endswith('.mp3') or fname.endswith('.wav'):
        fpath = os.path.join(NONAUDIO_DIR, fname)
        v = VoiceActivityDetector(fpath)
        detected = v.detect_speech()
        out_fpath = os.path.join(NONAUDIO_DIR, 'processed', fname[:-4] + '.txt')
        np.savetxt(out_fpath, detected)
