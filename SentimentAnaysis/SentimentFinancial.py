from transformers import RobertaTokenizer, RobertaForSequenceClassification
import torch
from flask import Flask, request, jsonify
from urllib.parse import unquote
from collections import deque

# Load tokenizer and model
model_name = "mrm8488/distilroberta-finetuned-financial-news-sentiment-analysis"
tokenizer = RobertaTokenizer.from_pretrained(model_name)
model = RobertaForSequenceClassification.from_pretrained(model_name)
app = Flask(__name__)

# Use a deque to store results with a maximum size of 2000 entries
result_cache = deque(maxlen=2000)

def analyze_sentiment(text):
    # Tokenize the input text
    input_ids = tokenizer.encode(text, return_tensors="pt")

    # Perform sentiment analysis
    with torch.no_grad():
        outputs = model(input_ids)

    # Get the predicted class (positive, negative, neutral)
    predicted_class = torch.argmax(outputs.logits).item()

    # Map the predicted class to sentiment label
    sentiment_labels = ['Negative', 'Neutral', 'Positive']
    sentiment = sentiment_labels[predicted_class]

    return sentiment

@app.route('/sentiment', methods=['POST'])
def get_sentiment():
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

    print(decoded_text)
    print(result)
    return jsonify({'sentiment': result})

if __name__ == '__main__':
    app.run(debug=True, port=2005)
