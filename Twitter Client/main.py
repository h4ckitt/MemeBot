import tweepy
import pandas as pd
import os
import random
import csv
from decouple import config


def write_to_csv(csv_file, payload):
	location = os.path.join(os.getcwd(), csv_file)
	file = open(location, 'w+', newline='')
	with file:
		write = csv.writer(file)
		write.writerows(payload)

	file.close()


def pick_split(csv_name):
	# This function picks a random meme from the csv file
	location = os.path.join(os.getcwd(), csv_name)
	data = pd.read_csv(location)
	data_list = [[x] for x in list(data.Full)]
	pick = random.choice(data_list)
	data_list.insert(0, ['Full'])
	data_list.remove(pick)	
	path, caption = os.path.join(os.getcwd(), 'images', pick[0].split('|')[0].strip()), pick[0].split('|')[1].strip()
	write_to_csv(csv_name, data_list)
	return path, caption


def send_tweet(image, text=' '):
	auth = tweepy.OAuthHandler(config('API_KEY'), config('API_SECRET'))
	auth.set_access_token(config('ACCESS_TOKEN'), config('ACCESS_SECRET'))
	api = tweepy.API(auth)
	media = api.media_upload(image)
	api.update_status(text, media_ids=[media.media_id])
	os.remove(image)


image_path, status = pick_split('config.csv')
send_tweet(image_path, status)

