using CommandsProj;
using CommandsProj.Commands.MoveCommands;
using IoCProj;
using ModelsProj;
using ModelsProj.Adapters;
using ModelsProj.Classes;
using ModelsProj.TypesObject;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tests
{

    internal class IoCTest
    {

        [Test]
        public void Test_RegisterAndResolve()
        {

            var obj = new SpaceShip();
            obj.SetProperty<Vector>("Velocity", new Vector(-7, 3));
            obj.SetProperty<Point>("Location", new Point(12, 5));
            var movingObjectAdapter = new MovingObjectAdapter(obj);

            IoC.Resolve<ICommand>("IoC.Register", "Move", (IoC.DependencyStrategy)((args) => {

                var spaceShipObj = (Uobject)args[0];
                var adapter = new MovingObjectAdapter(spaceShipObj);
                return new MoveCommand(adapter);

            })).Execute();

            IoC.Resolve<ICommand>("Move", obj).Execute();


            Assert.That(movingObjectAdapter.GetLocation().x, Is.EqualTo(5));
            Assert.That(movingObjectAdapter.GetLocation().y, Is.EqualTo(8));
        }

        [Test]
        public void Test_Scopes_Switching_By_Name()
        {

            IoC.Resolve<ICommand>("IoC.Register", "Weapon", (IoC.DependencyStrategy)((args) => "Laser")).Execute();
            Assert.That(IoC.Resolve<string>("Weapon"), Is.EqualTo("Laser"));

            IoC.Resolve<ICommand>("Scopes.New", "ScopeB").Execute();
            IoC.Resolve<ICommand>("IoC.Register", "Weapon", (IoC.DependencyStrategy)((args) => "Plasma")).Execute();
            Assert.That(IoC.Resolve<string>("Weapon"), Is.EqualTo("Plasma"));

            IoC.Resolve<ICommand>("Scopes.Current", "root").Execute();
            Assert.That(IoC.Resolve<string>("Weapon"), Is.EqualTo("Laser"));

            IoC.Resolve<ICommand>("Scopes.Current", "ScopeB").Execute();
            Assert.That(IoC.Resolve<string>("Weapon"), Is.EqualTo("Plasma"));
        }

        [Test]
        public void Test_Resolve_From_Named_Scope_Without_Switching_Current()
        {

            IoC.Resolve<ICommand>("Scopes.Current", "root").Execute();
            IoC.Resolve<ICommand>("IoC.Register", "ScopedWeapon", (IoC.DependencyStrategy)((args) => "Laser")).Execute();

            IoC.Resolve<ICommand>("Scopes.New", "ScopeB").Execute();
            IoC.Resolve<ICommand>("IoC.Register", "ScopedWeapon", (IoC.DependencyStrategy)((args) => "Plasma")).Execute();

            IoC.Resolve<ICommand>("Scopes.Current", "root").Execute();

            Assert.That(IoC.Resolve<string>("Scopes.ScopeB.ScopedWeapon"), Is.EqualTo("Plasma"));
            Assert.That(IoC.Resolve<string>("ScopedWeapon"), Is.EqualTo("Laser"));
        }
    }
}
