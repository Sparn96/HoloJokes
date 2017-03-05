import sys
import numpy as np
from keras.preprocessing import sequence
from keras.preprocessing.sequence import pad_sequences
from vad2 import VoiceActivityDetector
from train_audio import make_audio_model, MAX_SERIES_LENGTH
from train_words import make_word_model, MAX_SEQUENCE_LENGTH

audio_model, _, _ = make_audio_model()
audio_model.load_weights('weights_audio')
word_model, _, _, tokenizer = make_word_model()
word_model.load_weights('weights_words')


def audio_predict(fpath):
    """

    :param fpath: Path to wav file
    :return: Likelihood of a joke (between 0 and 1)
    """
    v = VoiceActivityDetector(fpath)
    detected = v.detect_speech()

    X = sequence.pad_sequences([detected[:, 1]], dtype=float, padding='post', maxlen=MAX_SERIES_LENGTH,
                               truncating='post', value=0.)

    X_reshaped = np.zeros(shape=[X.shape[0], X.shape[1], 1], dtype=float)
    X_reshaped[:, :, 0] = X[:, :]

    predict = audio_model.predict(X_reshaped)

    return predict[0][0]


def word_predict(transcription):
    """

    :param transcription: Transcribed sentence
    :return: Likelihood of a joke (between 0 and 1)
    """

    texts = [transcription]

    # finally, vectorize the text samples into a 2D integer tensor
    sequences = tokenizer.texts_to_sequences(texts)
    data = pad_sequences(sequences, maxlen=MAX_SEQUENCE_LENGTH)

    # split the data into a training set and a validation set
    indices = np.arange(data.shape[0])
    data = data[indices]

    predict = word_model.predict(data)

    return predict[0][1]


print('Waiting for input...')

for line in sys.stdin:
    wav_path, transcription = line.split('$')

    word_pred = word_predict(transcription)

    audio_pred = audio_predict(wav_path)

    print('Word pred: {}, audio pred: {}'.format(word_pred, audio_pred))
