using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinimalApi.Domain.ModelViews
{
    public struct Home
    {
        public string Message { get => "Bem vindo ao meu projeto"; }
        public string Documentation { get => "/swagger"; }
    }
}