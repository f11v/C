using System;
using System.Collections.Generic;

namespace C.Models;

public partial class Semestre
{
    public int SemestreId { get; set; }

    public int Año { get; set; }

    public string Periodo { get; set; } = null!;

    public virtual ICollection<Materia> Materia { get; set; } = new List<Materia>();
}
