namespace Queue.Rabbit.Core
{
    /// <summary>
    /// Модель создания exhcange и queue на стороне rabbit
    /// </summary>
    public class RabbitDeclare
    {
        /// <summary>
        /// Наименование очереди
        /// </summary>
        public string Queue { get; set; }
        /// <summary>
        /// Наименование exchange
        /// </summary>
        public string Exchange { get; set; }
    }
}
