FROM python:3.8-slim-buster

COPY . .
RUN apt-get update && apt install gcc -y
RUN pip install --no-cache-dir -r requirements.txt