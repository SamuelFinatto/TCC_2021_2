import amqp from "amqplib/callback_api"
const queue = 'hello';
import * as log4js from "log4js";
const logger = log4js.getLogger("RabbitManager");
logger.level = "debug";
export class RabbitManager{
    Connection : amqp.Connection;
    Channel : amqp.Channel;

    constructor(){
        amqp.connect('amqp://localhost', (error0, conn) => {
            if (error0) {
                throw error0;
            }

            this.Connection = conn;
            this.CreateChannel();
        });
    }

    public CreateChannel(){
        this.Connection.createChannel((error1, chann) => {
            if (error1) {
                throw error1;
            }

            this.Channel = chann;
            this.CreateQueue();
        });
    }

    public CreateQueue(){
        const msg = 'Hello world';

        this.Channel.assertQueue(queue, {
            durable: false
        });
    }

    public SendMessage(message: string){
        this.Channel.sendToQueue(queue, Buffer.from(message));
        logger.info("Message Sent");
    }

    private static _instance: RabbitManager;
    public static get Instance()
    {
        return this._instance || (this._instance = new this());
    }
}

export const rabbitManagerInstance = RabbitManager.Instance;
