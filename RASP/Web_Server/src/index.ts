import express from "express";
import dotenv from "dotenv";
import { rabbitManagerInstance } from "./Managers/RabbitManager";
// initialize configuration
dotenv.config();

// port is now available to the Node.js runtime
// as if it were an environment variable
const port = process.env.SERVER_PORT;

const app = express();


// // Configure Express to use EJS
// app.set( "views", path.join( __dirname, "views" ) );
// app.set( "view engine", "ejs" );

app.use(express.static(__dirname + '/views'));

app.get("/message", (req, res) => {
    rabbitManagerInstance.SendMessage("MEssageidj isjd");
    res.status(200).send("OK");
});

// start the express server
app.listen( port, () => {
    // tslint:disable-next-line:no-console
    console.log( `server started at http://localhost:${ port }` );
} );