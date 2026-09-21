import { screen } from "@testing-library/react";
import type { UserEvent } from "@testing-library/user-event";

export async function fillForm(
  user: UserEvent,
  fields: readonly { field: string; label: string }[],
  values: Record<string, string>,
) {
  for (const { field, label } of fields) {
    const input = screen.getByLabelText(label, { exact: true });
    await user.clear(input);
    if (values[field]) await user.type(input, values[field]);
  }
}

export async function replaceField(user: UserEvent, label: string, value: string) {
  const input = screen.getByLabelText(label, { exact: true });
  await user.clear(input);
  if (value) await user.type(input, value);
}
