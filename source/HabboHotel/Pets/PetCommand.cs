using System;
using System.Collections.Generic;

namespace Cyber.HabboHotel.Pets
{
	internal static class PetCommandHandler
	{
        internal static Dictionary<short, bool> GetPetCommands(Pet Pet)
        {
            Dictionary<short, bool> Output = new Dictionary<short, bool>();
            short qLevel = (short)Pet.Level;

            switch (Pet.Type)
            {
                default:
                case 0: // perro
                case 1: // gato
                case 2: // cocodrilo
                case 3: // Terrier
                case 4: // Oso
                case 5: // Jabali
                case 6: // León
                case 7: // Rhino
                    {
                        
                        Output.Add(0, true); // SIÉNTATE
                        Output.Add(1, true); // DESCANSA
                        Output.Add(13, true); // A CASA
                        Output.Add(2, qLevel >= 2); // TÚMBATE
                        Output.Add(4, qLevel >= 3); // PIDE
                        Output.Add(3, qLevel >= 4); // VEN AQUÍ
                        Output.Add(5, qLevel >= 4); // HAZ EL MUERTO
                        Output.Add(43, qLevel >= 5); // COMER
                        Output.Add(14, qLevel >= 5); // BEBE
                        Output.Add(6, qLevel >= 6); // QUIETO
                        Output.Add(17, qLevel >= 6); // FÚTBOL
                        Output.Add(8, qLevel >= 8); // LEVANTA
                        Output.Add(7, qLevel >= 9); // SÍGUEME
                        Output.Add(9, qLevel >= 11); // SALTA
                        Output.Add(11, qLevel >= 11); // JUEGA
                        Output.Add(12, qLevel >= 12); // CALLA
                        Output.Add(10, qLevel >= 12); // HABLA
                        Output.Add(15, qLevel >= 16); // IZQUIERDA
                        Output.Add(16, qLevel >= 16); // DERECHA
                        Output.Add(24, qLevel >= 17); // ADELANTE
                        
                        if (Pet.Type == 3 || Pet.Type == 4)
                        {
                            Output.Add(46, true);
                        }
                    }
                    break;

                case 8: // Araña
                    Output.Add(1, true); // DESCANSA
                    Output.Add(2, true); // TÚMBATE
                    Output.Add(3, qLevel >= 2); // VEN AQUÍ
                    Output.Add(17, qLevel >= 3); // FÚTBOL
                    Output.Add(6, qLevel >= 4); // QUIETO
                    Output.Add(5, qLevel >= 4); // HAZ EL MUERTO
                    Output.Add(7, qLevel >= 5); // SÍGUEME
                    Output.Add(23, qLevel >= 6); // ENCIENDE TV
                    Output.Add(9, qLevel >= 7); // SALTA
                    Output.Add(10, qLevel >= 8); // HABLA
                    Output.Add(11, qLevel >= 8); // JUEGA
                    Output.Add(24, qLevel >= 9); // ADELANTE
                    Output.Add(15, qLevel >= 10); // IZQUIERDA
                    Output.Add(16, qLevel >= 10); // DERECHA
                    Output.Add(13, qLevel >= 12); // A CASA
                    Output.Add(14, qLevel >= 13); // BEBE
                    Output.Add(19, qLevel >= 14); // BOTA
                    Output.Add(20, qLevel >= 14); // ESTATUA
                    Output.Add(22, qLevel >= 15); // GIRA
                    Output.Add(21, qLevel >= 16); // BAILA
                    break;

                case 16:
                    break;
            }

            return Output;
        }
	}
}
