using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using C.Models;

namespace C.Controllers
{
    public class CalificacionesController : Controller
    {
        private readonly CContext _context;

        public CalificacionesController(CContext context)
        {
            _context = context;
        }

        // GET: Calificaciones
        public async Task<IActionResult> Index()
        {
            // Obtener todas las calificaciones incluyendo las relaciones necesarias
            var calificaciones = await _context.Calificaciones
                .Include(c => c.Materia)
                    .ThenInclude(m => m.Semestre) // Incluir la relación con Semestre de Materia
                .Include(c => c.Usuario)
                .ToListAsync();

            // Agrupación de calificaciones por usuario, materia y semestre
            var calificacionesAgrupadas = calificaciones
                .GroupBy(c => new { c.UsuarioId, c.Materia.SemestreId, c.MateriaId })
                .Select(g => new
                {
                    UsuarioId = g.Key.UsuarioId,
                    SemestreId = g.Key.SemestreId,
                    MateriaId = g.Key.MateriaId,
                    Calificaciones = g.ToList()
                }).ToList();

            // Calcular promedio de descripción de materia y filtrar materias
            var materiasFiltradas = calificaciones
                .Select(c => new { Descripcion = Convert.ToDecimal(c.Materia.Descripcion) })
                .GroupBy(c => c.Descripcion > 0.6M) // Agrupar por si la descripción es mayor a 0.6
                .Where(g => g.Key) // Filtrar solo las materias cuya descripción es mayor a 0.6
                .SelectMany(g => g.Select(c => c.Descripcion))
                .ToList();

            // Filtrar calificaciones cuya nota general sea mayor a 9
            var calificacionesFiltradasMayorANueve = calificaciones
                .Where(c => c.NotaGeneral > 9)
                .ToList();

            // Filtrar las calificaciones por los semestres del año 2010 al 2015 y NotaGeneral hasta 6
            var calificacionesFiltradasPorAnio = calificaciones
                .Where(c => c.Materia.Semestre.Año >= 2010 && c.Materia.Semestre.Año <= 2015 && c.NotaGeneral <= 6)
                .OrderBy(c => c.NotaGeneral)
                .ToList();

            // Agrupar y organizar las calificaciones por año de semestre y estudiante
            var calificacionesAgrupadasPorAnio = calificacionesFiltradasPorAnio
                .GroupBy(c => c.Materia.Semestre.Año)
                .Select(g => new
                {
                    Año = g.Key,
                    Estudiantes = g.Select(c => new
                    {
                        Estudiante = c.Usuario.Nombre,
                        NotaGeneral = c.NotaGeneral,
                        Materias = c.Materia.NombreMateria
                    }).ToList(),
                    EstudiantesBajos = g.Count(c => c.NotaGeneral <= 6)
                })
                .OrderBy(g => g.Año)
                .ToList();

            // Contar el número de estudiantes con calificaciones bajas por materia
            var estudiantesBajosPorMateria = calificacionesFiltradasPorAnio
                .GroupBy(c => c.Materia.NombreMateria)
                .Select(g => new
                {
                    Materia = g.Key,
                    EstudiantesBajos = g.Count(c => c.NotaGeneral <= 6)
                })
                .ToList();

            // Pasar los datos a la vista
            ViewBag.CalificacionesAgrupadas = calificacionesAgrupadas;
            ViewBag.MateriasFiltradas = materiasFiltradas;
            ViewBag.CalificacionesFiltradasMayorANueve = calificacionesFiltradasMayorANueve;
            ViewBag.CalificacionesAgrupadasPorAnio = calificacionesAgrupadasPorAnio;
            ViewBag.EstudiantesBajosPorMateria = estudiantesBajosPorMateria;

            return View(calificacionesFiltradasPorAnio);
        }




        // GET: Calificaciones/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var calificacione = await _context.Calificaciones
                .Include(c => c.Materia)
                .Include(c => c.Usuario)
                .FirstOrDefaultAsync(m => m.CalificacionId == id);
            if (calificacione == null)
            {
                return NotFound();
            }

            return View(calificacione);
        }

        // GET: Calificaciones/Create
        public IActionResult Create()
        {
            ViewData["MateriaId"] = new SelectList(_context.Materias, "MateriaId", "MateriaId");
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "UsuarioId", "UsuarioId");
            return View();
        }

        // POST: Calificaciones/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CalificacionId,Nota1,Nota2,Nota3,NotaFinal,NotaExtra,UsuarioId,MateriaId,NotaGeneral")] Calificacione calificacione)
        {
            if (ModelState.IsValid)
            {
                // Calculo del promedio de las notas
                calificacione.NotaFinal = (calificacione.Nota1 + calificacione.Nota2 + calificacione.Nota3) / 3;

                calificacione.NotaExtra = calificacione.NotaFinal - 6;

                // Obtener la descripción de la materia
                var materia = await _context.Materias.FindAsync(calificacione.MateriaId);
                if (materia != null)
                {
                    // Convertir la descripción a decimal
                    if (decimal.TryParse(materia.Descripcion, out decimal descripcionDecimal))
                    {
                        // Calcular NotaGeneral
                        calificacione.NotaGeneral = ((calificacione.Nota1 + calificacione.Nota2)/2)*(1- descripcionDecimal) + (calificacione.Nota3 * descripcionDecimal);
                    }
                    else
                    {
                        ModelState.AddModelError("", "La descripción de la materia no es un número válido.");
                        ViewData["MateriaId"] = new SelectList(_context.Materias, "MateriaId", "MateriaId", calificacione.MateriaId);
                        ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "UsuarioId", "UsuarioId", calificacione.UsuarioId);
                        return View(calificacione);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "La materia no existe.");
                    ViewData["MateriaId"] = new SelectList(_context.Materias, "MateriaId", "MateriaId", calificacione.MateriaId);
                    ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "UsuarioId", "UsuarioId", calificacione.UsuarioId);
                    return View(calificacione);
                }

                _context.Add(calificacione);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MateriaId"] = new SelectList(_context.Materias, "MateriaId", "MateriaId", calificacione.MateriaId);
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "UsuarioId", "UsuarioId", calificacione.UsuarioId);
            return View(calificacione);
        }

        // GET: Calificaciones/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var calificacione = await _context.Calificaciones.FindAsync(id);
            if (calificacione == null)
            {
                return NotFound();
            }
            ViewData["MateriaId"] = new SelectList(_context.Materias, "MateriaId", "MateriaId", calificacione.MateriaId);
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "UsuarioId", "UsuarioId", calificacione.UsuarioId);
            return View(calificacione);
        }

        // POST: Calificaciones/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CalificacionId,Nota1,Nota2,Nota3,NotaFinal,NotaExtra,UsuarioId,MateriaId,NotaGeneral")] Calificacione calificacione)
        {
            if (id != calificacione.CalificacionId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Calculo del promedio de las notas
                    calificacione.NotaFinal = (calificacione.Nota1 + calificacione.Nota2 + calificacione.Nota3) / 3;

                    calificacione.NotaExtra = calificacione.NotaFinal - 6;

                    // Obtener la descripción de la materia
                    var materia = await _context.Materias.FindAsync(calificacione.MateriaId);
                    if (materia != null)
                    {
                        // Convertir la descripción a decimal
                        if (decimal.TryParse(materia.Descripcion, out decimal descripcionDecimal))
                        {
                            // Calcular NotaGeneral
                            calificacione.NotaGeneral = calificacione.NotaFinal * descripcionDecimal;
                        }
                        else
                        {
                            ModelState.AddModelError("", "La descripción de la materia no es un número válido.");
                            ViewData["MateriaId"] = new SelectList(_context.Materias, "MateriaId", "MateriaId", calificacione.MateriaId);
                            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "UsuarioId", "UsuarioId", calificacione.UsuarioId);
                            return View(calificacione);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "La materia no existe.");
                        ViewData["MateriaId"] = new SelectList(_context.Materias, "MateriaId", "MateriaId", calificacione.MateriaId);
                        ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "UsuarioId", "UsuarioId", calificacione.UsuarioId);
                        return View(calificacione);
                    }

                    _context.Update(calificacione);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CalificacioneExists(calificacione.CalificacionId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["MateriaId"] = new SelectList(_context.Materias, "MateriaId", "MateriaId", calificacione.MateriaId);
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "UsuarioId", "UsuarioId", calificacione.UsuarioId);
            return View(calificacione);
        }

        // GET: Calificaciones/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var calificacione = await _context.Calificaciones
                .Include(c => c.Materia)
                .Include(c => c.Usuario)
                .FirstOrDefaultAsync(m => m.CalificacionId == id);
            if (calificacione == null)
            {
                return NotFound();
            }

            return View(calificacione);
        }

        // POST: Calificaciones/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var calificacione = await _context.Calificaciones.FindAsync(id);
            if (calificacione != null)
            {
                _context.Calificaciones.Remove(calificacione);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CalificacioneExists(int id)
        {
            return _context.Calificaciones.Any(e => e.CalificacionId == id);
        }

        public async Task<IActionResult> AgruparCalificacionesPorSemestre(int usuarioId) { var calificacionesUsuario = await _context.Calificaciones.Include(c => c.Materia).Include(c => c.Usuario).Where(c => c.UsuarioId == usuarioId).ToListAsync(); var calificacionesAgrupadas = calificacionesUsuario.GroupBy(c => c.Materia.SemestreId).Select(g => new { SemestreId = g.Key, Materias = g.GroupBy(c => c.MateriaId).Select(mg => new { MateriaId = mg.Key, Calificaciones = mg.ToList() }).ToList() }).ToList(); return View(calificacionesAgrupadas); }
    }
}