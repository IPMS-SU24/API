using Amazon.SQS;
using Amazon.SQS.Model;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace IPMS.Business.Services
{
    public class MessageService : IMessageService
    {
        private static readonly string SQS_EMAIL = "MailSQS";
        private static readonly string SQS_NOTIFICATION = "NotificationSQS";
        private readonly IAmazonSQS _sqsClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<MessageService> _logger;
        public MessageService(IAmazonSQS sqsClient, IConfiguration configuration, ILogger<MessageService> logger)
        {
            _sqsClient = sqsClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendMessage<TMessage>(TMessage message) where TMessage : class
        {
            var queueUrl = await GetQueueUrl(message);
            var sendMessageRequest = new SendMessageRequest()
            {
                QueueUrl = queueUrl,
                MessageBody = JsonSerializer.Serialize(message)
            };
            await _sqsClient.SendMessageAsync(sendMessageRequest);
        }
        private async Task<string> GetQueueUrl<TMessage>(TMessage message)
        {
            var queueName = string.Empty;
            if(message is IPMSMailMessage)
            {
                queueName =  _configuration[SQS_EMAIL];
            }
            else if(message is NotificationMessage)
            {
                queueName = _configuration[SQS_NOTIFICATION];
            }

            try
            {
                var response = await _sqsClient.GetQueueUrlAsync(queueName);
                return response.QueueUrl;
            }
            catch (QueueDoesNotExistException ex)
            {
                _logger.LogError(ex, "SQS Queue for send is not existed!!!");
                throw;
            }
        }
    }
}
