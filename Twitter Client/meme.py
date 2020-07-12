import tweepy
import pandas as pd
import random
from decouple import config

df = pd.read_csv('test.csv', index_col='Index')
auth = tweepy.OAuthHandler(config('CONSUMER_KEY'), config('CONSUMER_SECRET'))
auth.set_access_token(config('ACCESS_KEY'), config('ACCESS_SECRET'))
api = tweepy.API(auth)

row = random.randrange(len(df))
path, caption = str(df['full'][row]).split('|')[0].strip(), str(df['full'][row]).split('|')[1].strip()


def send(image, description=''):
    media = api.media_upload(image)
    api.update_status(description, media_ids=[media.media_id])


send(path, caption)
new = df.drop(index=row)
new.to_csv('test.csv')
