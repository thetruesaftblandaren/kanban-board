interface Props {
    message: string | null;
}

export default function ErrorMessage({ message }: Props) {
    if (!message) return null;

    return <p style={{ color: "red"}}>{message}</p>;
}