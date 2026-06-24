// ---------------------------------------------------------------------------
// Ambient declarations — fallback for CI/sandbox where `npm install`
// did not write .d.ts files to the Windows filesystem mount.
// Safe to keep permanently — will be superseded by real types after install.
// ---------------------------------------------------------------------------

declare module 'lucide-react' {
  import { ComponentType, SVGAttributes } from 'react'
  export interface LucideProps extends SVGAttributes<SVGElement> {
    size?: number | string
    strokeWidth?: number | string
    absoluteStrokeWidth?: boolean
    className?: string
  }
  export type LucideIcon = ComponentType<LucideProps>
  export const BarChart3: LucideIcon
  export const Building2: LucideIcon
  export const ChevronRight: LucideIcon
  export const CreditCard: LucideIcon
  export const FileText: LucideIcon
  export const FileUp: LucideIcon
  export const LogOut: LucideIcon
  export const Pencil: LucideIcon
  export const Plus: LucideIcon
  export const Upload: LucideIcon
  export const X: LucideIcon
}

declare module 'react-hook-form' {
  import type { ReactElement } from 'react'

  export type FieldValues = Record<string, any>
  export type DefaultValues<T> = Partial<T>

  export interface RegisterOptions {
    required?: string | boolean
    pattern?: { value: RegExp; message: string }
    min?: number | string | { value: number; message: string }
    max?: number | string | { value: number; message: string }
    minLength?: number | { value: number; message: string }
    maxLength?: number | { value: number; message: string }
    validate?: ((v: unknown) => boolean | string) | Record<string, (v: unknown) => boolean | string>
    disabled?: boolean
  }

  export interface FieldError {
    type: string
    message?: string
  }

  export type FieldErrorsImpl<T extends FieldValues = FieldValues> = {
    [K in keyof T]?: FieldError
  }

  export interface UseFormRegister<T extends FieldValues> {
    (name: keyof T & string, options?: RegisterOptions): {
      name: string
      ref: (el: unknown) => void
      onChange: (e: unknown) => void
      onBlur: (e: unknown) => void
    }
  }

  export interface UseFormHandleSubmit<T extends FieldValues> {
    (onValid: (data: T) => void, onInvalid?: (errors: FieldErrorsImpl<T>) => void): (e?: unknown) => void
  }

  export interface UseFormReturn<T extends FieldValues> {
    register: UseFormRegister<T>
    handleSubmit: UseFormHandleSubmit<T>
    formState: { errors: FieldErrorsImpl<T>; isSubmitting: boolean }
    reset: (values?: Partial<T>) => void
    setValue: (name: keyof T & string, value: T[keyof T]) => void
    watch: (name?: keyof T & string) => T | T[keyof T]
  }

  export interface UseFormProps<T extends FieldValues> {
    defaultValues?: DefaultValues<T>
    mode?: 'onSubmit' | 'onBlur' | 'onChange' | 'onTouched' | 'all'
  }

  export function useForm<T extends FieldValues = FieldValues>(
    props?: UseFormProps<T>
  ): UseFormReturn<T>
  export type SubmitHandler<T extends FieldValues> = (data: T) => void | Promise<void>
}

declare module 'react-router-dom' {
  import type { ComponentType, ReactNode } from 'react'

  export interface RouteObject {
    path?: string
    element?: ReactNode
    children?: RouteObject[]
    index?: boolean
  }

  export interface RouterProviderProps {
    router: ReturnType<typeof createBrowserRouter>
  }

  export function createBrowserRouter(routes: RouteObject[]): unknown
  export function RouterProvider(props: RouterProviderProps): ReactNode
  export function Navigate(props: { to: string; replace?: boolean }): ReactNode
  export function Outlet(): ReactNode
  export function Link(props: { to: string; children?: ReactNode; className?: string; 'aria-label'?: string }): ReactNode
  export function useNavigate(): (to: string, options?: { replace?: boolean }) => void
  export function useLocation(): { pathname: string; search: string; hash: string; state: unknown }
}
