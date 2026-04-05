using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using CommandsProj;
using CommandsProj.CommandExceptions;
using CommandsProj.Commands.MoveCommands;
using Microsoft.VSDiagnostics;
using ModelsProj;
using ModelsProj.Adapters;
using ModelsProj.TypesObject;
using System;


[CPUUsageDiagnoser]
public class FuelBenchmark
{

    internal class CheckFuelComamndWithoutEx : ICommand
    {
        IFuelHaveObject _obj;

        public CheckFuelComamndWithoutEx(IFuelHaveObject obj)
        {
            _obj = obj;
        }

        public void Execute()
        {
            if (_obj.GetFuel() - _obj.GetFuelBurnVelocity() < 0)
                throw new FuelLowException();
        }
        public bool ExecuteWithoitEx()
        {
            return _obj.GetFuel() - _obj.GetFuelBurnVelocity() < 0 ;
      
        }
    }

    internal class FuelHaveObj : IFuelHaveObject
    {
        public float GetFuel()
        {
            return 1;
        }
        public void SetFuel(float fuel){ }

        public float GetFuelBurnVelocity() 
        { 
            return 1f; 
        }
    }


    [Params(100, 1000)]
    public int N;

    private float[] data;
    private SpaceShip spaceShip;
    private FuelHaveObj fuelObject;

    private readonly Consumer consumer = new Consumer();

    [GlobalSetup]
    public void Setup()
    {
        spaceShip = new SpaceShip();
        fuelObject = new FuelHaveObj();

        data = new float[N];
        Random r = new Random(42);
        for (int i = 0; i < data.Length; i++)
            data[i] = (float)r.NextDouble() * -10000;
    }

    [Benchmark(Description = "CheckFuelWithEx")]
    public void CheckFuel_Batch_Test()
    {
        for (int i = 0; i < data.Length; i++)
        {
            try
            {
                var command = new CheckFuelComamnd(fuelObject);
                command.Execute();
            }
            catch (FuelLowException)
            {
                // Обработка исключения
            }
        }
    }

    [Benchmark(Baseline = true, Description = "CheckFuelWithoutEx")]
    public void CheckFuel_Without_Ex_Batch_Test()
    {
        for (int i = 0; i < data.Length; i++)
        {
            var command = new CheckFuelComamndWithoutEx(fuelObject);
            bool success = command.ExecuteWithoitEx();

            // Сообщаем бенчмарку, что результат нам важен
            consumer.Consume(success);
        }
    }
}