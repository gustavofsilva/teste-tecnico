import { AbstractControl } from '@angular/forms';
export function fieldError(control: AbstractControl | null, label: string): string {
  if (!control?.touched || !control.errors) return '';
  if (control.hasError('required')) return `${label} é obrigatório.`;
  if (control.hasError('email')) return 'Informe um email válido.';
  if (control.hasError('minlength')) return `${label} deve ter no mínimo ${control.getError('minlength').requiredLength} caracteres.`;
  if (control.hasError('mismatch')) return 'As senhas não coincidem.';
  return `Verifique o campo ${label.toLowerCase()}.`;
}
