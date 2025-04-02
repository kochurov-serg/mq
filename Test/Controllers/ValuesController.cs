using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NATS.Client.Internals;
using Newtonsoft.Json;
using Queue.Core;
using Queue.Rabbit.Client;
using Queue.Rabbit.Client.Interfaces;
using Queue.Rabbit.Core.Options;
using Queue.Rabbit.Core.Repeat;

namespace Test.Controllers
{
    public class Person
    {
        public string Name { get; set; }

        public decimal Amount { get; set; }

        public Person Parent { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly ILogger<ValuesController> _logger;
        private readonly PersonalClient _queueClient;
        private readonly IRabbitQueueClient _rabbitQueue;

        private const string _content = @"
{
    ""transactionId"": ""027cc729-0e4a-4a54-b02b-16e1b314c940"",
    ""clientId"": 12,
    ""requestId"": ""487a03ac-effd-42f2-9c43-a0f07c0dc6c5"",
    ""orderId"": ""1"",
    ""paymentMethod"": ""creditCard"",
    ""paymentMode"": ""oneStagePayment"",
    ""paymentProvider"": ""sberbank"",
    ""status"": ""paid"",
    ""amount"": 11.92,
    ""currency"": ""rub"",
    ""signature"": ""6675f24727cc9ef3f8521ff2206c0e9001d33665f10f58ef54675a246f01d19d"",
    ""paymentDateTime"": ""2019-08-16T09:31:35.849582+03:00"",
    ""сreateDateTime"": ""2019-08-16T09:30:57.830782+03:00"",
    ""updateDateTime"": ""2019-08-16T09:30:57.830782+03:00"",
    ""cardInfo"": {
      ""mask"": ""555555**5599""
    },
    ""receipt"": {
      ""documentNumber"": ""6209"",
      ""sessionNumber"": ""9677"",
      ""number"": ""6538"",
      ""fiscalSign"": ""1770009545"",
      ""deviceNumber"": ""1400000000000099"",
      ""regNumber"": ""0000000400054952"",
      ""fiscalDriveNumber"": ""9999078900001341"",
      ""name"": ""ООО Мовиста"",
      ""inn"": ""9701094130"",
      ""receiptType"": ""income"",
      ""ofd"": ""ООО \""Ярус\"" (\""ОФД-Я\"")"",
      ""ofdInn"": ""7728699517"",
      ""documentDateTime"": ""2019-08-16T09:31:00+03:00"",
      ""receiptItems"": [
        {
          ""direction"": ""income"",
          ""name"": ""Позиция 3"",
          ""quantity"": 2,
          ""amount"": 3.41,
          ""vatType"": ""vat0""
        },
        {
          ""direction"": ""income"",
          ""name"": ""Позиция 2"",
          ""quantity"": 1,
          ""amount"": 2.76,
          ""vatType"": ""vat0""
        },
        {
          ""direction"": ""income"",
          ""name"": ""Позиция 1"",
          ""quantity"": 1,
          ""amount"": 2.34,
          ""vatType"": ""vat0""
        }
      ],
      ""url"": ""www.ofd-ya.ru"",
      ""fnsWebsite"": ""www.nalog.ru"",
      ""qrCodeData"": ""t=20190816T093100&s=11.92&fn=9999078900001341&i=6209&fp=1770009545&n=1"",
      ""receiptBasisType"": ""payment""
    },
    ""refunds"": []
  }";

        public ValuesController(ILogger<ValuesController> logger, IRabbitQueueClient rabbitQueue)
        {
            _logger = logger;
            //	_queueClient = queueClient;
            _rabbitQueue = rabbitQueue;
        }

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get(CancellationToken cancellationToken)
        {
            return new string[] { "value1", "value2" };
        }


        // GET api/values/5
        [HttpGet("{id}")]
        [ResponseCache(Duration = 0, NoStore = true)]
        public async Task<ActionResult> Get(int id)
        {
            foreach (var i in Enumerable.Range(1, 1000))
            {
                var person = new Person
                {
                    Name = "Ivan",
                    Amount = id,
                    Parent = new Person
                    {
                        Name = "Ilya",
                        Amount = i * 10
                    }
                };

                var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/api/values/postPerson")
                {
                    Content = new StringContent(JsonConvert.SerializeObject(person), Encoding.UTF8, "application/json")
                };

                var response = await _rabbitQueue.Send(request, CancellationToken.None);
                //var response = await _queueClient.Send(request, CancellationToken.None);
            }
            //var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/api/values/postPerson")
            //{
            //	Content = new StringContent(JsonConvert.SerializeObject(person), Encoding.UTF8, "application/json")
            //};
            //		request.Headers.AddCorrelation(Guid.NewGuid().ToString("N"));
            //request.AddRetry(new RepeatConfig
            //{
            //	Count = 3,
            //	Strategy = RepeatStrategy.Progression
            //});

            //request.AddRabbitRequestOption(new RabbitRequestOption
            //{
            //	Delay = TimeSpan.FromMinutes(1)
            //});
            //var response = await _queueClient.Send(request, CancellationToken.None);


            //	var pers = await response.Content.ReadAsAsync<Person>();
            return Ok();
        }

        // GET api/values/5
        [HttpGet("retry/{id}")]
        [ResponseCache(Duration = 0, NoStore = true)]
        public async Task<ActionResult> Retry(int id)
        {
            var person = new Person
            {
                Name = "Ivan",
                Amount = id,
                Parent = new Person
                {
                    Name = "Ilya",
                    Amount = id
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/api/values")
            {
                Content = new StringContent(JsonConvert.SerializeObject(person), Encoding.UTF8, "application/json")
            };
            //request.Headers.AddReply(Guid.NewGuid().ToString("N"));
            request.AddRetry(new RepeatConfig
            {
                Count = 3,
                Strategy = RepeatStrategy.Progression
            });

            //request.AddRabbitRequestOption(new RabbitRequestOption
            //{
            //	Delay = TimeSpan.FromMinutes(1)
            //});
            var response = await _rabbitQueue.Send(request, CancellationToken.None);


            //	var pers = await response.Content.ReadAsAsync<Person>();
            return Ok();
        }


        // GET api/values/5
        [HttpGet("reply/{id}")]
        [ResponseCache(Duration = 0, NoStore = true)]
        public async Task<ActionResult> Reply(int id)
        {
            var person = new Person
            {
                Name = "Ivan",
                Amount = id,
                Parent = new Person
                {
                    Name = "Ilya",
                    Amount = id
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/api/values/replyValue")
            {
                Content = new StringContent(JsonConvert.SerializeObject(person), Encoding.UTF8, "application/json")
            };
            request.Headers.AddReply(Guid.NewGuid().ToString("N"));
            request.AddRetry(new RepeatConfig
            {
                Count = 3,
                Strategy = RepeatStrategy.Progression
            });

            //request.AddRabbitRequestOption(new RabbitRequestOption
            //{
            //	Delay = TimeSpan.FromMinutes(1)
            //});
            var response = await _rabbitQueue.Send(request, CancellationToken.None);


            var pers = await response.Content.ReadAsAsync<Person>();
            return Ok(pers);
        }

        private static int _counter = 0;
        // POST api/values
        [HttpPost]
        public Task Post([FromBody] Person value)
        {
            _counter += 1;
            var iteration = _counter % 3;

            if (iteration == 1)
            {
                HttpContext.Response.StatusCode = 500;
                return Task.CompletedTask;
            }

            if (iteration == 0)
            {
                throw new Exception("Ошибка обработки ответа");
            }

            _logger.LogInformation(value.Name);
            return Task.CompletedTask;
        }        
        
        // POST api/values
        [HttpPost]
        [Route("replyValue")]
        public Person ReplyValue([FromBody] Person value)
        {
            _logger.LogInformation(value.Name);
            value.Amount += 1000;
            return value;
        }

        // POST api/values
        [HttpPost]
        [Route("postPerson")]
        public ActionResult PostPerson(Person value)
        {
            //return Redirect("http://ya.ru");
            return Created("http://localhost/values/v2/1", value);
            ///return StatusCode(500);
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
