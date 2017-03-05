import keras
import numpy as np
import os

from keras.layers import LSTM, Dense
from keras.models import Sequential
from keras.preprocessing import sequence
from sklearn import preprocessing
from sklearn.utils import shuffle

MAX_SERIES_LENGTH = 100
VALIDATION_SPLIT = 0.1
LSTM_UNITS = 150


def make_audio_model():
    np.random.seed(1235)

    JOKES_DIR = 'audio/jokes/processed'
    NONJOKES_DIR = 'audio/nonjokes/processed'

    jokes = os.listdir(JOKES_DIR)
    nonjokes = os.listdir(NONJOKES_DIR)

    n = len(jokes) + len(nonjokes)
    Y = np.zeros(shape=[n], dtype=int)

    i = 0
    a = []
    for fname in jokes:
        data = np.genfromtxt(os.path.join(JOKES_DIR, fname), dtype=float, delimiter=' ', usecols=[1])
        a.append(data)
        Y[i] = 1
        i += 1

    for fname in nonjokes:
        data = np.genfromtxt(os.path.join(NONJOKES_DIR, fname), dtype=float, delimiter=' ', usecols=[1])
        a.append(data)
        Y[i] = 0
        i += 1

    X = sequence.pad_sequences(a, dtype=float, padding='post', maxlen=MAX_SERIES_LENGTH, truncating='post', value=0.)

    X_reshaped = np.zeros(shape=[X.shape[0], X.shape[1], 1], dtype=float)
    X_reshaped[:, :, 0] = X[:, :]

    X, Y = shuffle(X_reshaped, Y)

    model = Sequential()
    model.add(LSTM(LSTM_UNITS, input_dim=1))  # try using a GRU instead, for fun
    model.add(Dense(1))

    # try using different optimizers and different optimizer configs
    model.compile(loss='binary_crossentropy',
                  optimizer='adam',
                  metrics=['accuracy'])

    return model, X, Y


if __name__ == '__main__':
    model, X, Y = make_audio_model()

    print('Train...')
    model.fit(X, Y, batch_size=25, nb_epoch=20,
              validation_split=VALIDATION_SPLIT)

    model.save_weights('weights_audio')

    print('Done.')
