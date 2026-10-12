interface Props {
  message: string | null;
  className?: string;
}

export default function ErrorMessage({ message, className = "" }: Props) {
  if (!message) return null;

  return (
    <p role="alert" className={`text-sm text-red-600 ${className}`}>
      {message}
    </p>
  );
}
