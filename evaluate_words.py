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
import random

import numpy as np
from keras.preprocessing.sequence import pad_sequences
from keras.utils.np_utils import to_categorical

from train_words import make_word_model, MAX_SEQUENCE_LENGTH

np.random.seed(1337)
random.seed(1337)

model, X, Y, tokenizer = make_word_model()

# happy learning!
model.load_weights('weights_words')


def make_predict_data():
    texts = []  # list of text samples
    labels = []  # list of label ids

    texts.append("What's the difference between a cow and a truck? The wheels.")
    labels.append(1)
    texts.append("Knock knock. Who's there? A watch!")
    labels.append(1)
    texts.append("I am the weakest link. Too bad this is not true for you.")
    labels.append(1)
    texts.append("Why did the chicken cross the road?")
    labels.append(1)
    texts.append("Why did the chicken cross the road?")
    labels.append(1)
    texts.append("Light travels faster than sound. This is why some people appear bright until you hear them speak.")
    labels.append(1)
    texts.append("Men have two emotions: Hungry and Horny. If you see him without an erection, make him a sandwich.")
    labels.append(1)
    texts.append("In the sixteenths century, Poland has seen a big influx in immigrants.")
    labels.append(0)
    texts.append("The united kingdom has a population of about 300 people")
    labels.append(0)
    texts.append("If you want to exit you can just go there.")
    labels.append(0)
    texts.append("Go to the homepage and download the song. It's an mp3.")
    labels.append(0)
    texts.append(
        "We've had a quick scout around the internet for the best one-liners we could find and these were the ones that made us chortle")
    labels.append(0)
    texts.append("This looks absolutely heavenly. Sometimes simpler is better.")
    labels.append(0)

    # finally, vectorize the text samples into a 2D integer tensor
    sequences = tokenizer.texts_to_sequences(texts)
    word_index = tokenizer.word_index
    data = pad_sequences(sequences, maxlen=MAX_SEQUENCE_LENGTH)

    labels = to_categorical(np.asarray(labels))

    # split the data into a training set and a validation set
    indices = np.arange(data.shape[0])
    data = data[indices]
    labels = labels[indices]

    x_train = data[:]
    y_train = labels[:]

    return x_train, y_train

x_train, y_train = make_predict_data()

predict = model.predict(x_train)

print('Predictions: \n' + str(predict))
