FROM mirror.gcr.io/node:25-alpine

WORKDIR /app

COPY package.json ./

RUN npm install --omit=dev

COPY index.js ./index.js

ENV NODE_ENV=production
ENV PORT=3000

EXPOSE 3000

USER node

CMD ["node", "index.js"]
