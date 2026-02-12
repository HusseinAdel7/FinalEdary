import { Pipe, PipeTransform, inject } from '@angular/core';
import { LOCALE_ID } from '@angular/core';

/**
 * يعرض النص حسب لغة الموقع: عربي أو إنجليزي.
 * إذا كانت اللغة عربية يعرض arabicValue، وإلا يعرض englishValue.
 * إذا القيمة المختارة فاضية يعرض الأخرى كبديل.
 */
@Pipe({ name: 'bilingualName', standalone: true })
export class BilingualNamePipe implements PipeTransform {
  private readonly locale = inject(LOCALE_ID);

  transform(arabicValue: string | null | undefined, englishValue: string | null | undefined): string {
    const ar = (arabicValue ?? '').trim();
    const en = (englishValue ?? '').trim();
    const isArabic = String(this.locale).toLowerCase().startsWith('ar');

    if (isArabic) {
      return ar || en || '-';
    }
    return en || ar || '-';
  }
}
