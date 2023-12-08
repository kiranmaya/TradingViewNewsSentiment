from transformers import pipeline
import torch
from transformers import AutoTokenizer, AutoModelForSequenceClassification
from flask import Flask, request, jsonify
from urllib.parse import unquote
from collections import deque
from datetime import datetime


sentiment_analysis = pipeline("sentiment-analysis",model="soleimanian/financial-roberta-large-sentiment")
print(sentiment_analysis("Kotak downbeat on Indian two-wheeler makers; says valuations 'irrational'"))
  