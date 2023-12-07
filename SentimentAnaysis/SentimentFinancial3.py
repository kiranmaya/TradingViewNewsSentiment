import torch
from transformers import AutoTokenizer, AutoModelForSequenceClassification
from flask import Flask, request, jsonify
from urllib.parse import unquote
from collections import deque
from datetime import datetime

print(f"Started {datetime.now().strftime('%H:%M:%S')}")
model_name = "RashidNLP/Finance-Sentiment-Classification"
tokenizer = None

model = None
def load_model():
    global tokenizer, model
    if tokenizer is None or model is None:
        print(f"Loading started for model {datetime.now().strftime('%H:%M:%S')}")
        tokenizer = AutoTokenizer.from_pretrained(model_name)
        model = AutoModelForSequenceClassification.from_pretrained(model_name)
        print(f"Loading Done ")

load_model()
print(f"Starting Flask Server {datetime.now().strftime('%H:%M:%S')} ")
app = Flask(__name__)
# Use a deque to store results with a maximum size of 2000 entries
result_cache = deque(maxlen=4000)

def analyze_sentiment(text):
    # Tokenize the input text
    inputs = tokenizer(text, return_tensors="pt")

    # Perform sentiment analysis
    with torch.no_grad():
        outputs = model(**inputs)

    # Get the predicted class (positive, negative, neutral)
    predicted_class = torch.argmax(outputs.logits, dim=1).item()

    # Map the predicted class to sentiment label
    sentiment_labels = ['negative', 'neutral', 'positive']
    sentiment = sentiment_labels[predicted_class]

    return sentiment

@app.route('/sentiment', methods=['POST'])
def get_sentiment():
    print(f"sentiment start {datetime.now().strftime('%H:%M:%S')}")
    data = request.get_json()
    text_to_analyze = data.get('text', '')
    decoded_text = unquote(text_to_analyze)
    # Check if result is already in cache
    for cached_text, result in result_cache:
        if cached_text == decoded_text:
            break
    else:
        # Perform sentiment analysis if not in cache
        result = analyze_sentiment(decoded_text)
        # Store result in cache
        result_cache.append((decoded_text, result))

    # Move print statements outside the else block
    print(decoded_text)
    print(result)
    print(f"sentiment End {datetime.now().strftime('%H:%M:%S')}")
    print("")
    print("")
    print("")
    return jsonify({'sentiment': result})

if __name__ == '__main__':
    app.run(debug=True, port=2005)
