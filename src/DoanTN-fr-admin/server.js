// const { createServer } = require('https');
// const { parse } = require('url');
// const next = require('next');
// const fs = require('fs');
// const path = require('path');

// const PORT = process.env.PORT || 4201;
// const HOST = 'localhost';
// const dev = process.env.NODE_ENV !== 'production';

// const app = next({ dev });
// const handle = app.getRequestHandler();

// const sslDirectory = path.join(__dirname, 'ssl');

// const httpsOptions = {
//   key: fs.readFileSync(path.join(sslDirectory, 'localhost-key.pem')),
//   cert: fs.readFileSync(path.join(sslDirectory, 'localhost.pem')),
// };

// app.prepare()
//   .then(() => {
//     createServer(httpsOptions, (req, res) => {
//       const parsedUrl = parse(req.url, true);
//       handle(req, res, parsedUrl);
//     }).listen(PORT, () => {
//       console.log(`🚀 Server ready at: https://${HOST}:${PORT}`);
//       console.log(`🔧 Mode: ${dev ? 'Development' : 'Production'}`);
//     });
//   })
//   .catch((err) => {
//     console.error('❌ Failed to start server', err);
//     process.exit(1);
//   });

const { createServer } = require('http');
const { parse } = require('url');
const next = require('next');

const PORT = process.env.PORT || 4201;
const HOST = 'localhost';
const dev = process.env.NODE_ENV !== 'production';

const app = next({ dev });
const handle = app.getRequestHandler();

app.prepare()
  .then(() => {
    createServer((req, res) => {
      const parsedUrl = parse(req.url, true);
      handle(req, res, parsedUrl);
    }).listen(PORT, HOST, () => {
      console.log(`🚀 Server ready at: http://${HOST}:${PORT}`);
      console.log(`🔧 Mode: ${dev ? 'Development' : 'Production'}`);
    });
  })
  .catch((err) => {
    console.error('❌ Failed to start server', err);
    process.exit(1);
  });

