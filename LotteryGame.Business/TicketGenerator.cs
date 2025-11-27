using LotteryGame.Core.Configurations;
using LotteryGame.Core.Interfaces;
using LotteryGame.Core;

namespace LotteryGame.Business
{
    public class TicketGenerator : ITicketGenerator
    {
        private readonly IRandomGenerator _randomGenerator;
        private readonly HashSet<string> _generatedNumbers = new();

        public TicketGenerator(IRandomGenerator randomGenerator)
        {
            _randomGenerator = randomGenerator ?? throw new ArgumentNullException(nameof(randomGenerator));
        }

        public IEnumerable<Ticket> GenerateTickets(int numberOfTickets, LotteryConfig config)
        {
            if (numberOfTickets <= 0)
                throw new ArgumentException("Number of tickets must be greater than 0", nameof(numberOfTickets));

            var tickets = new List<Ticket>();

            for (int i = 0; i < numberOfTickets; i++)
            {
                tickets.Add(GenerateTicket(config));
            }

            return tickets;
        }

        public Ticket GenerateTicket(LotteryConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            string ticketNumber;
            int attempts = 0;
            const int maxAttempts = 1000;

            do
            {
                ticketNumber = _randomGenerator.Next(config.MinTicketNumber, config.MaxTicketNumber + 1).ToString();
                attempts++;

                if (attempts > maxAttempts)
                {
                    _generatedNumbers.Clear();
                    attempts = 0;
                }
            }
            while (_generatedNumbers.Contains(ticketNumber));

            _generatedNumbers.Add(ticketNumber);

            return new Ticket { Number = ticketNumber };
        }

        public bool IsValidTicketNumber(string ticketNumber, LotteryConfig config)
        {
            if (string.IsNullOrEmpty(ticketNumber) || config == null)
                return false;

            if (!int.TryParse(ticketNumber, out int number))
                return false;

            return number >= config.MinTicketNumber && number <= config.MaxTicketNumber;
        }
    }

}
