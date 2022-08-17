const { app, BrowserWindow, Menu, globalShortcut, ipcMain, dialog } = require('electron')
const { createHash, createCipheriv, createDecipheriv } = require('crypto');
const path = require('path');


function createWindow () {
    Menu.setApplicationMenu(null)
    const win = new BrowserWindow({
        width: 800,
        height: 410,
        title: "Fast Crypt",
        webPreferences: {
            nodeIntegration: true,
            contextIsolation: false,
            enableRemoteModule: true
        }
    })

    win.loadFile("index.html");

    ipcMain.on("data",(event,data) => {
        event.sender.send("rply", EncNow(data[0],data[1]));
    });
    // s = Secure
    ipcMain.on("sdata",(event,data) => {
      event.sender.send("srply", DecNow(data[0],data[1]));
    });
}

function EncNow(pass,dat) {
  const keyE = Buffer.from(createHash('sha256').update(pass).digest('hex'),"hex");
  const ivE = Buffer.from(createHash('md5').update(pass).digest('hex'),"hex");
  const cipher = createCipheriv('aes-256-cbc',keyE,ivE);
  const encryptedMessage = cipher.update(dat, 'utf8', 'base64') + cipher.final('base64');
  return encryptedMessage;
}

function DecNow(pass,dat) {
  try {
    const key = Buffer.from(createHash('sha256').update(pass).digest('hex'),"hex");
    const iv = Buffer.from(createHash('md5').update(pass).digest('hex'),"hex");
    const decipher = createDecipheriv('aes-256-cbc', key, iv);
    const decryptedMessage = decipher.update(dat, 'base64', 'utf-8') + decipher.final('utf8');
    return decryptedMessage.toString('utf-8');
  }
  catch (error) {
    dialog.showMessageBox({
      title: 'Error',
      buttons: ['Close'],
      type: 'error',
      message: 'An error occurred while decrypting.'
    })
    return null;
  }
}

app.whenReady().then(() => {
  createWindow()

  app.on('activate', () => {
    if (BrowserWindow.getAllWindows().length === 0) {
      createWindow()
    }
  })
})

app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') {
    app.quit()
  }
})