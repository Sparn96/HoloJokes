'''This script loads pre-trained word embeddings (GloVe embeddings)
into a frozen Keras Embedding layer, and uses it to
train a text classification model on the 20 Newsgroup dataset
(classication of newsgroup messages into 20 different categories).

GloVe embedding data can be found at:
http://nlp.stanford.edu/data/glove.6B.zip
(source page: http://nlp.stanford.edu/projects/glove/)

20 Newsgroup data can be found at:
http://www.cs.cmu.edu/afs/cs.cmu.edu/project/theo-20/www/data/news20.html
'''

from __future__ import print_function
import os
import numpy as np

np.random.seed(1337)

from keras.preprocessing.text import Tokenizer
from keras.preprocessing.sequence import pad_sequences
from keras.utils.np_utils import to_categorical
from keras.layers import Dense, Input, Flatten
from keras.layers import Conv1D, MaxPooling1D, Embedding
from keras.models import Model
import sys
import pickle

BASE_DIR = ''
GLOVE_DIR = BASE_DIR + 'glove.6B/'
JOKE_DIR = BASE_DIR + 'joke-data/'
JOKE_FNAMES = ['humorous_jokes.pickle', 'short_oneliners.pickle']
NONJOKE_FNAMES = ['short_wiki_sentences.pickle', 'movie_dialogs.txt']
MAX_SEQUENCE_LENGTH = 1000
MAX_NB_WORDS = 20000
EMBEDDING_DIM = 100
VALIDATION_SPLIT = 0.2


def make_word_model():
    # first, build index mapping words in the embeddings set
    # to their embedding vector

    print('Indexing word vectors.')

    embeddings_index = {}
    f = open(os.path.join(GLOVE_DIR, 'glove.6B.100d.txt'))
    line = f.readline()
    while line:
        try:
            # TODO remove
            # if len(embeddings_index) > 100:
            #     break
            if line != '???':
                values = line.split()
                word = values[0]
                coefs = np.asarray(values[1:], dtype='float32')
                embeddings_index[word] = compile
            line = f.readline()
        except Exception as e:
            line = '???'

    f.close()

    print('Found %s word vectors.' % len(embeddings_index))

    # second, prepare text samples and their labels
    print('Processing text dataset')

    texts = []  # list of text samples
    labels = []  # list of label ids

    for name in JOKE_FNAMES:
        path = os.path.join(JOKE_DIR, name)
        with open(path, 'rb') as f:
            b = pickle.load(f, encoding='latin1')
            for l in b:
                texts.append(l)
                labels.append(1)

    for name in NONJOKE_FNAMES:
        path = os.path.join(JOKE_DIR, name)
        mode = 'r' if name.endswith('.txt') else 'rb'
        with open(path, mode) as f:
            if name.endswith('.txt'):
                b = []
                l = f.readline()
                while l:
                    b.append(l)
                    try:
                        l = f.readline()
                    except UnicodeDecodeError:
                        l = '???'
            else:
                b = pickle.load(f, encoding='latin1')
            if len(b) > 30000:
                b = b[:30000]
            for l in b:
                texts.append(l)
                labels.append(0)

    print('Found %s texts.' % len(texts))

    # finally, vectorize the text samples into a 2D integer tensor
    tokenizer = Tokenizer(nb_words=MAX_NB_WORDS)
    tokenizer.fit_on_texts(texts)
    sequences = tokenizer.texts_to_sequences(texts)

    word_index = tokenizer.word_index
    print('Found %s unique tokens.' % len(word_index))

    data = pad_sequences(sequences, maxlen=MAX_SEQUENCE_LENGTH)

    labels = to_categorical(np.asarray(labels))
    print('Shape of data tensor:', data.shape)
    print('Shape of label tensor:', labels.shape)

    # split the data into a training set and a validation set
    indices = np.arange(data.shape[0])
    np.random.shuffle(indices)
    data = data[indices]
    labels = labels[indices]

    x_train = data[:]
    y_train = labels[:]

    print('Preparing embedding matrix.')

    # prepare embedding matrix
    nb_words = min(MAX_NB_WORDS, len(word_index))
    embedding_matrix = np.zeros((nb_words, EMBEDDING_DIM))
    for word, i in word_index.items():
        if i >= MAX_NB_WORDS:
            continue
        embedding_vector = embeddings_index.get(word)
        if embedding_vector is not None:
            try:
                # words not found in embedding index will be all-zeros.
                embedding_matrix[i] = embedding_vector
            except Exception as e:
                pass

    # load pre-trained word embeddings into an Embedding layer
    # note that we set trainable = False so as to keep the embeddings fixed
    embedding_layer = Embedding(nb_words,
                                EMBEDDING_DIM,
                                weights=[embedding_matrix],
                                input_length=MAX_SEQUENCE_LENGTH,
                                trainable=False)

    # train a 1D convnet with global maxpooling
    sequence_input = Input(shape=(MAX_SEQUENCE_LENGTH,), dtype='int32')
    embedded_sequences = embedding_layer(sequence_input)
    x = Conv1D(128, 5, activation='relu')(embedded_sequences)
    x = MaxPooling1D(5)(x)
    x = Conv1D(128, 5, activation='relu')(x)
    x = MaxPooling1D(5)(x)
    x = Conv1D(128, 5, activation='relu')(x)
    x = MaxPooling1D(35)(x)
    x = Flatten()(x)
    x = Dense(50, activation='relu')(x)
    preds = Dense(2, activation='softmax')(x)
    
    model = Model(sequence_input, preds)
    model.compile(loss='binary_crossentropy',
                  optimizer='adam', metrics=['accuracy'])

    return model, x_train, y_train, tokenizer


if __name__ == '__main__':
    model, X, Y, _ = make_word_model()

    # happy learning!
    stat = model.fit(X, Y, nb_epoch=2, batch_size=128, validation_split=VALIDATION_SPLIT)

    model.save_weights('weights_words')

    print('done. ' + str(stat))
