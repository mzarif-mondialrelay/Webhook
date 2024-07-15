This is a sample project to demonstrate how you could read and authenticate the request that you are going to receive from the Mondial Relay webhook application.
The code is available in the RequestConsumerController.cs in the method ConsumeRequest. 
You will find in the code 2 types of validations that you can do: 

1- Time stamp validation, where you could verify the time of the reauest and you can set this value to suit your preference
2- Signature, and it is used to validate that the request is issued from a trusted source, where the timestamp and the payload are combined and then digested using HMAC

The payload is the body of the request and it contains the fields of the message that you will receive
