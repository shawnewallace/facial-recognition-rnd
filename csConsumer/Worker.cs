using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace csConsumer
{
	public class Worker : BackgroundService
	{
		private const string SERVICE_URL = "https://sqs.us-east-1.amazonaws.com";
		private const string QUEUE_URL = "https://sqs.us-east-1.amazonaws.com/809991377783/camp-2020-face-detected";

		private readonly ILogger<Worker> _logger;

		public Worker(ILogger<Worker> logger)
		{
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			var config = new AmazonSQSConfig();
			config.ServiceURL = SERVICE_URL;
			using (var sqsClient = new AmazonSQSClient(config))
			{
				while (!stoppingToken.IsCancellationRequested)
				{
					// _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

					var receiveRequest = new ReceiveMessageRequest();
					receiveRequest.QueueUrl = QUEUE_URL;
					receiveRequest.MaxNumberOfMessages = 1;

					var receiveMessageResponse = await sqsClient.ReceiveMessageAsync(receiveRequest, stoppingToken);

					foreach(var message in receiveMessageResponse.Messages)
					{
						ProcessMessage(message);

						var deleteMessageRequest = new DeleteMessageRequest();

						deleteMessageRequest.QueueUrl = QUEUE_URL;
						deleteMessageRequest.ReceiptHandle = message.ReceiptHandle;

						var response = await sqsClient.DeleteMessageAsync(deleteMessageRequest);
					}


					await Task.Delay(1000, stoppingToken);
				}
			}
		}

		protected void ProcessMessage(Message message)
		{
			var user = JsonSerializer.Deserialize<EntranceMusicUser>(message.Body);

			_logger.LogInformation(JsonSerializer.Serialize(user));
		}
	}

	public class EntranceMusicUser
	{
		public AwsIdentifier name{get;set;}
		public AwsIdentifier artist{get;set;}
		public AwsIdentifier song { get; set; }
	}

	public class AwsIdentifier
	{
		public string S {get;set;}
	}
}
