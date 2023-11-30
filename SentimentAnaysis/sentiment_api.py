from flask import Flask, request, jsonify
from nltk.sentiment import SentimentIntensityAnalyzer
import nltk
from urllib.parse import unquote

app = Flask(__name__)

# Download the VADER lexicon for sentiment analysis
nltk.download('vader_lexicon')

def analyze_sentiment(text):
    sia = SentimentIntensityAnalyzer()
    sentiment_scores = sia.polarity_scores(text)
    if sentiment_scores['compound'] >= 0.05:
        return 'Positive'
    elif sentiment_scores['compound'] <= -0.05:
        return 'Negative'
    else:
        return 'Neutral'

@app.route('/sentiment', methods=['POST'])
def get_sentiment():
    data = request.get_json()
    text_to_analyze = data.get('text', '')
    decoded_text = unquote(text_to_analyze)

    result = analyze_sentiment(decoded_text)
    print (decoded_text)
    print (result)
    return jsonify({'sentiment': result})

if __name__ == '__main__':
    app.run(debug=True, port=2005)
