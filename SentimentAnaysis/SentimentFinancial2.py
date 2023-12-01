import torch
from transformers import AutoTokenizer, AutoModelForSequenceClassification
from flask import Flask, request, jsonify
from urllib.parse import unquote
from collections import deque

# Load tokenizer and model
model_name = "Sigma/financial-sentiment-analysis"
tokenizer = AutoTokenizer.from_pretrained(model_name)
model = AutoModelForSequenceClassification.from_pretrained(model_name)
app = Flask(__name__)
# Use a deque to store results with a maximum size of 2000 entries
result_cache = deque(maxlen=2000)

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
    return jsonify({'sentiment': result})

if __name__ == '__main__':
    app.run(debug=True, port=2005)
