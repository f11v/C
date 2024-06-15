using System;
using System.Collections.Generic;

namespace C.Models;

public partial class Calificacione
{
    public int CalificacionId { get; set; }

    public decimal? Nota1 { get; set; }

    public decimal? Nota2 { get; set; }

    public decimal? Nota3 { get; set; }

    public decimal? NotaFinal { get; set; }

    public decimal? NotaExtra { get; set; }

    public int? UsuarioId { get; set; }

    public int? MateriaId { get; set; }

    public decimal? NotaGeneral { get; set; }

    public virtual Materia? Materia { get; set; }

    public virtual Usuario? Usuario { get; set; }
}
