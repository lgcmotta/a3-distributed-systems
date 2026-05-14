FROM mirror.gcr.io/python:3.14-alpine

WORKDIR /app

COPY requirements.txt ./

RUN pip install --no-cache-dir --requirement requirements.txt

COPY main.py ./main.py

ENV PYTHONDONTWRITEBYTECODE=1
ENV PYTHONUNBUFFERED=1
ENV FLASK_ENV=production
ENV PORT=5000

EXPOSE 5000

CMD ["python", "main.py"]