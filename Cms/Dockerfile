FROM node:18-alpine
WORKDIR /app

ENV NEXT_TELEMETRY_DISABLED 1

COPY package*.json ./

RUN npm install --legacy-peer-deps

COPY . .

RUN npm run build

ENV NODE_ENV=production

RUN npm install -g serve

EXPOSE 3000

CMD ["npx", "next", "start"]