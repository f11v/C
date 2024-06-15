using System;
using System.Collections.Generic;

namespace C.Models;

public partial class Materia
{
    public int MateriaId { get; set; }

    public string NombreMateria { get; set; } = null!;

    public string? Descripcion { get; set; }

    public int? SemestreId { get; set; }

    public virtual ICollection<Calificacione> Calificaciones { get; set; } = new List<Calificacione>();

    public virtual Semestre? Semestre { get; set; }

    public virtual ICollection<Usuario> Estudiantes { get; set; } = new List<Usuario>();

    public virtual ICollection<Usuario> Profesors { get; set; } = new List<Usuario>();
}
