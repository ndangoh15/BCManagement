using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Concurrency
{
    public static class OcrExecutionGate
    {
        // Ajustable selon tests (4 conseillé pour ton serveur)
        public static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1, 1);
    }
}
